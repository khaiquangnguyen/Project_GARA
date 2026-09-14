using System;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using MoreMountains.Tools;
using Spine.Unity;
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
        private InputAction _cancelChainedAction;

        private CombatParticipant _actor;
        private AttackExecutor _actorExecutor;
        private bool _phaseActive;

        private PhaseActionState _phaseActionState;
        private bool _chainedActionUsedThisPhase;
        private Action _activeChainedActionCancelHandler;

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
            _cancelChainedAction = map.FindAction("CancelChainedAction");
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
            _endOrLockCombo.performed += OnEndCombatPhase;
            _cancelChainedAction.performed += OnCancelChainedAction;
            CombatParticipant.Defeated += OnParticipantDefeated;

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
            _endOrLockCombo.performed -= OnEndCombatPhase;
            _cancelChainedAction.performed -= OnCancelChainedAction;
            CombatParticipant.Defeated -= OnParticipantDefeated;

            combatInputActions.FindActionMap("Combat").Disable();
        }

        // Drops the just-defeated participant from whatever's currently
        // selectable (a no-op if it wasn't in the pool) and refreshes the
        // turn-order display — fires the instant a participant dies,
        // regardless of whose turn it is or what killed them.
        private void OnParticipantDefeated(CombatParticipant participant)
        {
            targetSelector.RemoveCandidate(participant);
            RaiseTurnOrderChanged();
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
            if (CanChooseNewAction())
            {
                targetSelector.CycleLeft();
            }
        }

        private void OnTargetRight(InputAction.CallbackContext ctx)
        {
            if (CanChooseNewAction())
            {
                targetSelector.CycleRight();
            }
        }

        private void OnEndCombatPhase(InputAction.CallbackContext ctx)
        {
            if (!CanChooseNewAction())
            {
                return;
            }

            EndCombatPhase();
        }

        private void OnCancelChainedAction(InputAction.CallbackContext ctx)
        {
            if (!CanAct() || _phaseActionState != PhaseActionState.ChainedAction)
            {
                return;
            }

            _activeChainedActionCancelHandler?.Invoke();
        }

        private bool CanAct()
        {
            return _phaseActive && _actor != null && _actor.faction == FactionTag.Player && !_actorExecutor.IsBusy;
        }


        // Claims the phase's one chained-action slot for whichever driver
        // calls this on its own first input (basic attack today). Returns
        // false if the slot is already taken or already spent this phase.
        // onCancelRequested is invoked if the player presses Escape while
        // this chain owns the slot — each driver owns its own "how do I
        // unwind cleanly" logic; this layer only tracks whether the slot
        // is taken.
        private bool TryEnterChainedAction(Action onCancelRequested)
        {
            if (_phaseActionState != PhaseActionState.Regular || _chainedActionUsedThisPhase)
            {
                return false;
            }

            _phaseActionState = PhaseActionState.ChainedAction;
            _activeChainedActionCancelHandler = onCancelRequested;
            return true;
        }

        // Called by a driver once its chain is fully done — a finisher
        // landed, or it was explicitly cancelled. Spends the phase's slot
        // and hands control back to Regular.
        private void ExitChainedAction()
        {
            _phaseActionState = PhaseActionState.Regular;
            _chainedActionUsedThisPhase = true;
            _activeChainedActionCancelHandler = null;
        }

        private bool CanChooseNewAction()
        {
            return CanAct() && _phaseActionState == PhaseActionState.Regular;
        }

        private void ResetPhaseActionStateForNewPhase()
        {
            _phaseActionState = PhaseActionState.Regular;
            _chainedActionUsedThisPhase = false;
            _activeChainedActionCancelHandler = null;
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
            if (!CanChooseNewAction())
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

            AnnounceTargetingForSpecial(_actor, entry, targets);
            SpecialUsedAnnouncement?.Invoke();
            _actorExecutor.PlayAction(entry.state, targets, entry.positionMode);
            _actorExecutor.ActionFinished += OnSpecialActionFinished;

            void OnSpecialActionFinished()
            {
                _actorExecutor.ActionFinished -= OnSpecialActionFinished;
                AnnounceTargetingClearedForSpecial(_actor, entry);
                _actorExecutor.ReturnToStandardPosition();
            }
        }

        private void BeginPhaseForCurrentActor()
        {
            _actor = _battle.CurrentActor;
            _actorExecutor = _executors[_actor];
            SetActorSortingOrder(_actor, 1);
            TurnFactionChanged?.Invoke(_actor.faction == FactionTag.Player);
            ResetPhaseActionStateForNewPhase();
            ResetBasicAttackStateForNewPhase();
            _actor.basicAttackController.ResetForNewTurn();

            if (_actor.faction != FactionTag.Player)
            {
                _phaseActive = false;
                _enemyTurnController.TakeTurn(
                    _actor,
                    _actorExecutor,
                    LivingEnemiesOf(_actor),
                    target => AnnounceTargetingForSingleEnemyTarget(_actor, target),
                    () => AnnounceTargetingClearedForEnemies(_actor),
                    EndCombatPhase);
                return;
            }

            _phaseActive = true;
            targetSelector.BeginSelection(LivingEnemiesOf(_actor));
        }

        private void EndCombatPhase()
        {
            _phaseActive = false;
            targetSelector.EndSelection();
            AnnounceTargetingClearedForEnemies(_actor);
            SetActorSortingOrder(_actor, 0);

            _battle.AdvanceTurn();
            RaiseTurnOrderChanged();

            if (_battle.IsBattleOver)
            {
                return;
            }

            BeginPhaseForCurrentActor();
        }

        // Announces every OTHER living enemy as not-targeted, and the
        // selected one as targeted, for whichever single-enemy-target basic
        // attack is about to play. No-op for specials that hit everyone in
        // a pool (AllEnemy/AllFriendly) — see
        // AnnounceTargetingForSpecial/AnnounceTargetingClearedForSpecial for
        // those. This class has no opinion on what "not targeted" looks
        // like — it just broadcasts the targeting state via MMEventManager;
        // whatever's listening (see OnNotTargetedEffect) decides that.
        private void AnnounceTargetingForSingleEnemyTarget(CombatParticipant actor, CombatParticipant selectedTarget)
        {
            foreach (var enemy in LivingEnemiesOf(actor))
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(enemy.SceneRoot, enemy == selectedTarget));
            }
        }

        private void AnnounceTargetingClearedForEnemies(CombatParticipant actor)
        {
            foreach (var enemy in LivingEnemiesOf(actor))
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(enemy.SceneRoot, true));
            }
        }

        private void AnnounceTargetingForSpecial(CombatParticipant actor, SpecialAttackEntry entry, IReadOnlyList<ICombatTarget> targets)
        {
            switch (entry.targetMode)
            {
                case SpecialTargetMode.OneEnemy:
                    foreach (var enemy in LivingEnemiesOf(actor))
                    {
                        MMEventManager.TriggerEvent(new TargetedStateEvent(enemy.SceneRoot, targets.Contains(enemy)));
                    }
                    break;
                case SpecialTargetMode.OneFriendly:
                    foreach (var ally in LivingAlliesOf(actor))
                    {
                        MMEventManager.TriggerEvent(new TargetedStateEvent(ally.SceneRoot, targets.Contains(ally)));
                    }
                    break;
                // AllEnemy/AllFriendly: everyone in the pool is targeted — nothing to announce as excluded.
            }
        }

        private void AnnounceTargetingClearedForSpecial(CombatParticipant actor, SpecialAttackEntry entry)
        {
            switch (entry.targetMode)
            {
                case SpecialTargetMode.OneEnemy:
                    foreach (var enemy in LivingEnemiesOf(actor))
                    {
                        MMEventManager.TriggerEvent(new TargetedStateEvent(enemy.SceneRoot, true));
                    }
                    break;
                case SpecialTargetMode.OneFriendly:
                    foreach (var ally in LivingAlliesOf(actor))
                    {
                        MMEventManager.TriggerEvent(new TargetedStateEvent(ally.SceneRoot, true));
                    }
                    break;
            }
        }

        // Renders the acting character above the rest of their formation
        // for the duration of their turn — order 1 while it's their turn,
        // reset to 0 once it ends. Sorting order itself lives on the
        // MeshRenderer Spine renders through, but resolved off the
        // SkeletonRenderer specifically (its own GameObject's renderer) so
        // this targets the actual skeleton's renderer, not just whichever
        // MeshRenderer happens to be found first under the character.
        private void SetActorSortingOrder(CombatParticipant actor, int order)
        {
            if (actor?.SceneRoot == null)
            {
                return;
            }

            var skeletonRenderer = actor.SceneRoot.GetComponentInChildren<SkeletonRenderer>();
            if (skeletonRenderer == null)
            {
                return;
            }

            var meshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = order;
            }
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
