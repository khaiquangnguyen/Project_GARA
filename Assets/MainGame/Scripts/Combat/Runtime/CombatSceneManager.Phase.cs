using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // The player-facing turn loop: one Combat Phase per turn, in which
    // specials (A/S/D/F) are repeatable — each gated only by AP+MP
    // affordability and the actor's executor being idle between uses — until
    // the player ends the phase with EndPhase (Enter). The enemy target
    // selector is shown continuously for the whole Combat Phase (see
    // BeginPhaseForCurrentActor) rather than opened per action, and a
    // special applies its own targeting rule the instant it's chosen:
    // OneEnemy uses the selector's current target, OneFriendly targets the
    // first living ally, and AllEnemy/AllFriendly hit every living member of
    // the relevant party. Non-player turns go to PerformEnemyTurn (see
    // .EnemyTurn) — no input is read, nothing is ticked, for them.
    public partial class CombatSceneManager
    {
        // Raised the instant a stunned (food-coma'd) actor's turn is skipped
        // — carries the actor's portrait so UI can show a "turn skipped"
        // announcement.
        public static event Action<Sprite> TurnSkipped;

        private const int TurnOrderDisplaySlotCount = 6;

        [Header("Combat Phase")]
        [SerializeField] private InputActionAsset combatInputActions;
        [SerializeField] private TargetSelector targetSelector;
        [SerializeField] private SkillCardInputHost skillCardInputHost;
        [SerializeField] private float stunnedTurnDisplaySeconds = 0.6f;

        private BattleContext _battle;
        private Dictionary<CombatParticipant, AttackExecutor> _executors;
        private ISkillInputSession _activeSession;
        private PhaseActionState _phaseActionState;

        private InputAction _slotA;
        private InputAction _slotB;
        private InputAction _slotC;
        private InputAction _slotD;
        private InputAction _targetLeft;
        private InputAction _targetRight;
        private InputAction _endPhase;

        private CombatParticipant _actor;
        private AttackExecutor _actorExecutor;
        private bool _phaseActive;

        private void AwakePhase()
        {
            var map = combatInputActions.FindActionMap("Combat");
            _slotA = map.FindAction("SlotA");
            _slotB = map.FindAction("SlotB");
            _slotC = map.FindAction("SlotC");
            _slotD = map.FindAction("SlotD");
            _targetLeft = map.FindAction("TargetLeft");
            _targetRight = map.FindAction("TargetRight");
            _endPhase = map.FindAction("EndPhase");
        }

        private void EnablePhase()
        {
            _slotA.performed += OnSlotA;
            _slotB.performed += OnSlotB;
            _slotC.performed += OnSlotC;
            _slotD.performed += OnSlotD;
            _targetLeft.performed += OnTargetLeft;
            _targetRight.performed += OnTargetRight;
            _endPhase.performed += OnEndCombatPhase;
            CombatParticipant.Defeated += OnParticipantDefeated;

            combatInputActions.FindActionMap("Combat").Enable();
        }

        private void DisablePhase()
        {
            _slotA.performed -= OnSlotA;
            _slotB.performed -= OnSlotB;
            _slotC.performed -= OnSlotC;
            _slotD.performed -= OnSlotD;
            _targetLeft.performed -= OnTargetLeft;
            _targetRight.performed -= OnTargetRight;
            _endPhase.performed -= OnEndCombatPhase;
            CombatParticipant.Defeated -= OnParticipantDefeated;

            combatInputActions.FindActionMap("Combat").Disable();

            AbortActiveSkillCardSession();
        }

        // Called by CombatManager once every participant is spawned and bound.
        public void BeginBattle(BattleContext battle, Dictionary<CombatParticipant, AttackExecutor> executors)
        {
            _battle = battle;
            _executors = executors;

            _battle.InitializeTurnOrder();
            RaiseTurnOrderChanged();
            BeginPhaseForCurrentActor();
        }

        // Safety rail: cuts short whatever skill-card input session is
        // running (if any) and resets the flow back to Regular. Called from
        // EndCombatPhase and OnDisable so a phase never ends, and this
        // component never gets disabled, mid-minigame.
        private void AbortActiveSkillCardSession()
        {
            if (_activeSession != null)
            {
                _activeSession.Abort();
                _activeSession = null;
            }

            _phaseActionState = PhaseActionState.Regular;
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

            RefreshTurnOrder(portraits);
        }

        private void OnSlotA(InputAction.CallbackContext ctx) => TryUseSkillCard(0);
        private void OnSlotB(InputAction.CallbackContext ctx) => TryUseSkillCard(1);
        private void OnSlotC(InputAction.CallbackContext ctx) => TryUseSkillCard(2);
        private void OnSlotD(InputAction.CallbackContext ctx) => TryUseSkillCard(3);

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

        private void OnEndCombatPhase(InputAction.CallbackContext ctx)
        {
            if (!CanAct())
            {
                return;
            }

            EndCombatPhase();
        }

        private bool CanAct()
        {
            return _phaseActive
                   && _actor != null
                   && _actor.faction == FactionTag.Player
                   && !_actorExecutor.IsBusy
                   && _phaseActionState == PhaseActionState.Regular;
        }

        // Fires the instant it's chosen — applies whatever targeting rule
        // the card itself declares, with no separate confirmation step.
        // OneEnemy reads the always-visible selector's current target;
        // OneFriendly always targets the first living ally (there's no
        // opportunity to pre-aim a friendly target since nothing keeps the
        // ally pool on screen ahead of time); All* ignores the selector
        // entirely and hits every living member of the relevant party.
        // Resources are spent immediately, before the card's input minigame
        // runs — there's no refund unless the card opts into refundOnAbort
        // (see OnSkillCardInputCompleted). While a card's minigame/
        // resolution is in progress (_phaseActionState != Regular), slots,
        // target cycling and EndPhase are all ignored — see CanAct.
        private void TryUseSkillCard(int slot)
        {
            if (!CanAct())
            {
                return;
            }

            if (!_actor.definition.TryGetSkillCard(slot, out var card) || card == null)
            {
                return;
            }

            IReadOnlyList<ICombatTarget> targets;
            switch (card.targetMode)
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

            if (!_actor.TrySpendResources(card.apCost, card.mpCost))
            {
                return;
            }

            AnnounceTargetingForSkillCard(_actor, card, targets);
            ShowSpecialUsedAnnouncement();

            _phaseActionState = PhaseActionState.SkillCardInput;
            skillCardInputHost.RaiseInputPhaseStarted(card);

            _activeSession = card.CreateInputSession(skillCardInputHost);
            _activeSession.Begin(performance => OnSkillCardInputCompleted(card, targets, performance));
        }

        // Fires once the card's input minigame finishes (including an
        // abort). WasAborted + refundOnAbort skips the animation/effects
        // entirely and refunds the spend; otherwise the swing plays and
        // every authored effect resolves at the animation's impact frame
        // (see AttackExecutor.PlayAction's onImpact).
        private void OnSkillCardInputCompleted(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets, SkillPerformance performance)
        {
            _activeSession = null;
            _phaseActionState = PhaseActionState.SkillCardResolving;
            skillCardInputHost.RaiseInputPhaseEnded(performance);

            if (performance.WasAborted && card.refundOnAbort)
            {
                _actor.RefundResources(card.apCost, card.mpCost);
                AnnounceTargetingClearedForSkillCard(_actor, card);
                _phaseActionState = PhaseActionState.Regular;
                return;
            }

            var battleQuery = _battle.QueryFor(_actor);
            var actingParticipant = _actor;
            _actorExecutor.PlayAction(card.animationState, targets, card.positionMode, onImpact: () =>
            {
                foreach (var effect in card.effects)
                {
                    if (effect != null)
                    {
                        effect.Resolve(new SkillEffectContext(battleQuery, actingParticipant, targets, performance));
                    }
                }

                actingParticipant.NotifySkillCardResolved(battleQuery, card, targets, performance);
            });
            _actorExecutor.ActionFinished += OnSkillCardActionFinished;

            void OnSkillCardActionFinished()
            {
                _actorExecutor.ActionFinished -= OnSkillCardActionFinished;
                AnnounceTargetingClearedForSkillCard(_actor, card);
                _actorExecutor.ReturnToStandardPosition();
                _phaseActionState = PhaseActionState.Regular;
            }
        }

        private void BeginPhaseForCurrentActor()
        {
            _actor = _battle.CurrentActor;
            _actorExecutor = _executors[_actor];
            SetActorSortingOrder(_actor, 1);
            ToggleTurnAnnouncement(_actor.faction == FactionTag.Player);

            // Timed modifiers (see TimedStatModifierSkillEffect) tick down
            // at the start of THEIR OWNER's own turn, not on every global
            // turn. If that changed the actor's Speed, the not-yet-acted
            // remainder of the turn order needs to be re-sorted.
            var speedBeforeTick = _actor.GetCurrentStats().Speed.Value;
            _actor.TickTimedModifiers();
            if (_actor.GetCurrentStats().Speed.Value != speedBeforeTick)
            {
                _battle.RecalculateTurnOrder();
            }

            // Cooking (Chef): a food-coma'd actor's statuses tick here, at
            // the start of their own phase, same as timed modifiers above.
            // If they're still stunned they never read input or take an
            // enemy turn — their whole phase is just the coma's own damage
            // tick and a brief display beat.
            _actor.TickStatuses();

            if (_actor.IsStunned)
            {
                _phaseActive = false;
                StartCoroutine(SkipStunnedTurn());
                return;
            }

            if (_actor.faction != FactionTag.Player)
            {
                _phaseActive = false;
                StartCoroutine(PerformEnemyTurn(_actor, _actorExecutor, LivingEnemiesOf(_actor)));
                return;
            }

            _phaseActive = true;
            targetSelector.BeginSelection(LivingEnemiesOf(_actor));
        }

        // A stunned actor takes its food-coma damage, burns one turn of the
        // status, and ends its phase without ever reading input or taking
        // an enemy turn.
        private IEnumerator SkipStunnedTurn()
        {
            var participant = _actor;
            TurnSkipped?.Invoke(participant.definition.portrait);
            var comaDamage = participant.PendingSkippedTurnDamage;
            if (comaDamage > 0)
            {
                participant.ApplyStatusTickDamage(comaDamage);
            }

            participant.ConsumeSkippedTurn();
            yield return new WaitForSeconds(stunnedTurnDisplaySeconds);
            EndCombatPhase();
        }

        private void EndCombatPhase()
        {
            AbortActiveSkillCardSession();

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
        // selected one as targeted, for whichever single-enemy-target
        // action is about to play. No-op for cards that hit everyone in a
        // pool (AllEnemy/AllFriendly) — see
        // AnnounceTargetingForSkillCard/AnnounceTargetingClearedForSkillCard
        // for those. This class has no opinion on what "not targeted" looks
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

        private void AnnounceTargetingForSkillCard(CombatParticipant actor, SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets)
        {
            switch (card.targetMode)
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

        private void AnnounceTargetingClearedForSkillCard(CombatParticipant actor, SkillCardDefinition card)
        {
            switch (card.targetMode)
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
