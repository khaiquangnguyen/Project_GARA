using System;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // Drives the player-facing turn loop: one merged Combat Phase per turn,
    // in which specials (A/S/D/F) are repeatable (gated by AP+MP) and the
    // basic-attack combo (Z/X/C) may be attempted exactly once, until the
    // player ends the phase with Enter. The enemy target selector is shown
    // continuously for the whole Combat Phase (see BeginPhaseForCurrentActor)
    // rather than opened per action — Z/X/C immediately starts/continues the
    // real-time basic-attack buffer/combo chain against whatever's currently
    // selected (see the CombatPhaseController.BasicAttack.cs partial), and a
    // special simply applies its own targeting rule the instant it's chosen:
    // OneEnemy uses the selector's current target, OneFriendly targets the
    // first living ally, and AllEnemy/AllFriendly hit every living member of
    // the relevant party. Non-player turns are handed off to
    // IEnemyTurnController — no input is read, nothing is ticked, for them.
    public partial class CombatPhaseController : MonoBehaviour
    {
        // Raised whenever the upcoming turn queue changes (a turn was
        // consumed, or a group rolled over), portrait per slot left-to-right
        // with slot 0 always being whoever's turn it currently is — kept as
        // a plain Sprite[] (not CombatParticipant[]) so UI code can listen
        // without depending on GARA.Combat types.
        public static event Action<Sprite[]> TurnOrderChanged;

        // Raised whenever the acting faction changes — true for a player
        // turn, false for an enemy turn — so UI can toggle a "Your Turn" /
        // "Enemy Turn" announcement without depending on GARA.Combat types.
        public static event Action<bool> TurnFactionChanged;

        // Raised the instant a special is used — a dedicated signal (not a
        // generic message) so UI can toggle its own "Special!" announcement
        // GameObject, the same way TurnFactionChanged toggles the turn one.
        public static event Action SpecialUsedAnnouncement;

        private const int TurnOrderDisplaySlotCount = 6;

        [SerializeField] private InputActionAsset combatInputActions;
        [SerializeField] private TargetSelector targetSelector;
        [SerializeField] private MonoBehaviour enemyTurnControllerSource; // must implement IEnemyTurnController

        private IEnemyTurnController _enemyTurnController;
        private BattleContext _battle;
        private Dictionary<CombatParticipant, AttackExecutor> _executors;

        private InputAction _atk1;
        private InputAction _atk2;
        private InputAction _atk3;
        private InputAction _slotA;
        private InputAction _slotB;
        private InputAction _slotC;
        private InputAction _slotD;
        private InputAction _targetLeft;
        private InputAction _targetRight;
        private InputAction _endOrLockCombo;

        private CombatParticipant _actor;
        private AttackExecutor _actorExecutor;
        private bool _phaseActive;

        private void Awake()
        {
            _enemyTurnController = enemyTurnControllerSource as IEnemyTurnController;

            var map = combatInputActions.FindActionMap("Combat");
            _atk1 = map.FindAction("Atk1");
            _atk2 = map.FindAction("Atk2");
            _atk3 = map.FindAction("Atk3");
            _slotA = map.FindAction("SlotA");
            _slotB = map.FindAction("SlotB");
            _slotC = map.FindAction("SlotC");
            _slotD = map.FindAction("SlotD");
            _targetLeft = map.FindAction("TargetLeft");
            _targetRight = map.FindAction("TargetRight");
            _endOrLockCombo = map.FindAction("EndOrLockCombo");
        }

        private void OnEnable()
        {
            _atk1.performed += OnAtk1;
            _atk2.performed += OnAtk2;
            _atk3.performed += OnAtk3;
            _slotA.performed += OnSlotA;
            _slotB.performed += OnSlotB;
            _slotC.performed += OnSlotC;
            _slotD.performed += OnSlotD;
            _targetLeft.performed += OnTargetLeft;
            _targetRight.performed += OnTargetRight;
            _endOrLockCombo.performed += OnEndOrLockCombo;

            combatInputActions.FindActionMap("Combat").Enable();
        }

        private void OnDisable()
        {
            _atk1.performed -= OnAtk1;
            _atk2.performed -= OnAtk2;
            _atk3.performed -= OnAtk3;
            _slotA.performed -= OnSlotA;
            _slotB.performed -= OnSlotB;
            _slotC.performed -= OnSlotC;
            _slotD.performed -= OnSlotD;
            _targetLeft.performed -= OnTargetLeft;
            _targetRight.performed -= OnTargetRight;
            _endOrLockCombo.performed -= OnEndOrLockCombo;

            combatInputActions.FindActionMap("Combat").Disable();
        }

        public void Initialize(BattleContext battle, Dictionary<CombatParticipant, AttackExecutor> executors)
        {
            _battle = battle;
            _executors = executors;
        }

        public void BeginBattle()
        {
            _battle.InitializeTurnOrder();
            RaiseTurnOrderChanged();
            BeginPhaseForCurrentActor();
        }

        // Slot 0 is always whoever's turn it currently is — the queue
        // itself shifts left as each turn is consumed, rather than a fixed
        // per-round slot being highlighted in place.
        private void RaiseTurnOrderChanged()
        {
            var upcoming = _battle.GetUpcomingQueue(TurnOrderDisplaySlotCount);
            var portraits = new Sprite[upcoming.Count];
            for (var i = 0; i < upcoming.Count; i++)
            {
                portraits[i] = upcoming[i].definition.portrait;
            }

            TurnOrderChanged?.Invoke(portraits);
        }

        private void OnSlotA(InputAction.CallbackContext ctx) => TryUseSpecial(0);
        private void OnSlotB(InputAction.CallbackContext ctx) => TryUseSpecial(1);
        private void OnSlotC(InputAction.CallbackContext ctx) => TryUseSpecial(2);
        private void OnSlotD(InputAction.CallbackContext ctx) => TryUseSpecial(3);

        private void OnTargetLeft(InputAction.CallbackContext ctx)
        {
            if (CanAct())
            {
                targetSelector.CycleLeft();
            }
        }

        private void OnTargetRight(InputAction.CallbackContext ctx)
        {
            if (CanAct())
            {
                targetSelector.CycleRight();
            }
        }

        private void OnEndOrLockCombo(InputAction.CallbackContext ctx)
        {
            if (!CanAct())
            {
                return;
            }

            if (_comboInProgress && !_comboUsedThisPhase)
            {
                // Locks the combo without a finisher; the phase continues.
                _comboInProgress = false;
                _comboUsedThisPhase = true;
            }
            else
            {
                EndCombatPhase();
            }
        }

        private bool CanAct()
        {
            return _phaseActive && _actor != null && _actor.faction == FactionTag.Player && !_actorExecutor.IsBusy;
        }

        // Fires the instant it's chosen — applies whatever targeting rule
        // the special itself declares, with no separate confirmation step.
        // OneEnemy reads the always-visible selector's current target;
        // OneFriendly always targets the first living ally (there's no
        // opportunity to pre-aim a friendly target since nothing keeps the
        // ally pool on screen ahead of time); All* ignores the selector
        // entirely and hits every living member of the relevant party.
        private void TryUseSpecial(int slot)
        {
            if (!CanAct())
            {
                return;
            }

            if (!_actor.definition.TryGetSpecial(slot, out var entry))
            {
                return;
            }

            if (_actor.currentAp < entry.apCost || _actor.currentMp < entry.mpCost)
            {
                return;
            }

            IReadOnlyList<ICombatTarget> targets;
            switch (entry.targetMode)
            {
                case SpecialTargetMode.AllEnemy:
                    targets = LivingEnemiesOf(_actor).Cast<ICombatTarget>().ToArray();
                    break;
                case SpecialTargetMode.AllFriendly:
                    targets = LivingAlliesOf(_actor).Cast<ICombatTarget>().ToArray();
                    break;
                case SpecialTargetMode.OneEnemy:
                    if (!targetSelector.HasCandidates)
                    {
                        return;
                    }
                    targets = new ICombatTarget[] { targetSelector.CurrentTarget };
                    break;
                case SpecialTargetMode.OneFriendly:
                    var allies = LivingAlliesOf(_actor);
                    if (allies.Count == 0)
                    {
                        return;
                    }
                    targets = new ICombatTarget[] { allies[0] };
                    break;
                default:
                    return;
            }

            if (!_actor.TrySpendResources(entry.apCost, entry.mpCost))
            {
                return;
            }

            SpecialUsedAnnouncement?.Invoke();
            _actorExecutor.PlayAction(entry.state, targets);
        }

        private void BeginPhaseForCurrentActor()
        {
            _actor = _battle.CurrentActor;
            _actorExecutor = _executors[_actor];
            TurnFactionChanged?.Invoke(_actor.faction == FactionTag.Player);
            ResetBasicAttackStateForNewPhase();
            _actor.basicAttackController.ResetForNewTurn();

            if (_actor.faction != FactionTag.Player)
            {
                _phaseActive = false;
                _enemyTurnController.TakeTurn(_actor, EndCombatPhase);
                return;
            }

            _phaseActive = true;
            targetSelector.BeginSelection(LivingEnemiesOf(_actor));
        }

        private void EndCombatPhase()
        {
            _phaseActive = false;
            targetSelector.EndSelection();

            _battle.AdvanceTurn();
            RaiseTurnOrderChanged();

            if (_battle.IsBattleOver)
            {
                return;
            }

            BeginPhaseForCurrentActor();
        }

        private List<CombatParticipant> LivingEnemiesOf(CombatParticipant actor)
        {
            var opposing = actor.faction == FactionTag.Player ? _battle.enemyParty : _battle.playerParty;
            return opposing.LivingMembers().ToList();
        }

        private List<CombatParticipant> LivingAlliesOf(CombatParticipant actor)
        {
            var own = actor.faction == FactionTag.Player ? _battle.playerParty : _battle.enemyParty;
            return own.LivingMembers().ToList();
        }
    }
}
