using System;
using System.Collections;
using GARA.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    public partial class CombatPhaseController
    {
        public static event Action BasicAttackStartedAnnouncement;
        public static event Action ComboPerformedAnnouncement;

        [Tooltip("Pause after the combo finisher's animation finishes before the basic-attack chain actually ends (targeting clears, actor returns to standard position) — keeps the finisher from immediately getting cut off.")]
        [SerializeField]
        private float comboFinisherEndDelay = 1f;

        private bool _basicAttackChainActive;

        private void OnAtk1(InputAction.CallbackContext ctx) => TryTriggerBasicAttack(AttackInput.Atk1);
        private void OnAtk2(InputAction.CallbackContext ctx) => TryTriggerBasicAttack(AttackInput.Atk2);
        private void OnAtk3(InputAction.CallbackContext ctx) => TryTriggerBasicAttack(AttackInput.Atk3);

        private void Update()
        {
            if (!CanAct() || _chainedActionUsedThisPhase)
            {
                return;
            }

            _actor.basicAttackController.Tick(true, OnBasicAttackResolved);
        }

        private void TryTriggerBasicAttack(AttackInput input)
        {
            if (!CanAct() || _chainedActionUsedThisPhase)
            {
                return;
            }

            if (!_basicAttackChainActive && _phaseActionState != PhaseActionState.Regular)
            {
                return;
            }

            if (!_actor.definition.TryGetBasicAttack(input, out var entry) || !_actor.TrySpendAp(entry.apCost))
            {
                return;
            }

            if (!_basicAttackChainActive)
            {
                _basicAttackChainActive = TryEnterChainedAction(OnBasicAttackChainCancelled);
                BasicAttackStartedAnnouncement?.Invoke();
            }

            _actor.basicAttackController.OnAttackInput(input);
        }

        private void OnBasicAttackResolved(ComboResolution resolution)
        {
            if (resolution.kind == ComboResolutionKind.None)
            {
                return;
            }

            PlayBasicAttackSwing(resolution);
        }

        // Single entry point for playing any basic-attack swing, whether
        // it's an ordinary input (from OnBasicAttackResolved) or a combo
        // finisher queued to play right after the input that triggered it
        // (from OnBasicAttackActionFinished, below) — a finisher never
        // bypasses that input's own swing, it plays once it's done.
        private void PlayBasicAttackSwing(ComboResolution resolution)
        {
            var selectedTarget = targetSelector.CurrentTarget;
            AnnounceTargetingForSingleEnemyTarget(_actor, selectedTarget);
            _actorExecutor.PlayAction(resolution.state, new ICombatTarget[] { selectedTarget }, resolution.positionMode);
            _actorExecutor.ActionFinished += OnBasicAttackActionFinished;

            if (resolution.kind == ComboResolutionKind.Finisher)
            {
                ComboPerformedAnnouncement?.Invoke();
                ExitBasicAttackChain();
            }
        }

        private void OnBasicAttackActionFinished()
        {
            _actorExecutor.ActionFinished -= OnBasicAttackActionFinished;

            if (_basicAttackChainActive && _actor.basicAttackController.TryTakeQueuedFinisher(out var finisher))
            {
                PlayBasicAttackSwing(finisher);
                return;
            }

            if (!_basicAttackChainActive)
            {
                StartCoroutine(FinishBasicAttackChainAfterDelay());
            }
        }

        private IEnumerator FinishBasicAttackChainAfterDelay()
        {
            yield return new WaitForSeconds(comboFinisherEndDelay);
            FinishBasicAttackChain();
        }

        private void OnBasicAttackChainCancelled()
        {
            ExitBasicAttackChain();
            FinishBasicAttackChain();
        }

        private void FinishBasicAttackChain()
        {
            AnnounceTargetingClearedForEnemies(_actor);
            _actorExecutor.ReturnToStandardPosition();
        }

        private void ExitBasicAttackChain()
        {
            _basicAttackChainActive = false;
            ExitChainedAction();
        }

        private void ResetBasicAttackStateForNewPhase()
        {
            _basicAttackChainActive = false;
        }
    }
}
