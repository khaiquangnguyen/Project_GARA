using System;
using GARA.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // Basic-attack (Z/X/C) handling for CombatPhaseController. The target
    // selector is shown continuously for the whole Combat Phase (see
    // BeginPhaseForCurrentActor) — pressing Z/X/C immediately starts or
    // continues the real-time buffer/combo chain against whatever's
    // currently selected, exactly like the original real-time combo system;
    // there's no separate target-selection/confirmation step for basic
    // attacks. The combo may still only be used once per phase — locked in
    // either by a matched finisher or by the player pressing Enter with no
    // pending selection (see the main partial's OnEndOrLockCombo).
    public partial class CombatPhaseController
    {
        // Raised the instant a basic-attack sequence starts (the first
        // Z/X/C of the phase, not every chained swing) — a dedicated signal
        // so UI can toggle its own announcement GameObject, the same way
        // TurnFactionChanged toggles the turn one.
        public static event Action BasicAttackStartedAnnouncement;

        private bool _comboInProgress;
        private bool _comboUsedThisPhase;

        private void OnAtk1(InputAction.CallbackContext ctx) => TryTriggerBasicAttack(AttackInput.Atk1);
        private void OnAtk2(InputAction.CallbackContext ctx) => TryTriggerBasicAttack(AttackInput.Atk2);
        private void OnAtk3(InputAction.CallbackContext ctx) => TryTriggerBasicAttack(AttackInput.Atk3);

        private void Update()
        {
            if (!CanAct() || _comboUsedThisPhase)
            {
                return;
            }

            _actor.basicAttackController.Tick(Time.deltaTime, true, OnBasicAttackResolved);
        }

        private void TryTriggerBasicAttack(AttackInput input)
        {
            if (!CanAct() || _comboUsedThisPhase)
            {
                return;
            }

            if (!_actor.definition.TryGetBasicAttack(input, out var entry) || !_actor.TrySpendAp(entry.apCost))
            {
                return;
            }

            if (!_comboInProgress)
            {
                BasicAttackStartedAnnouncement?.Invoke();
            }

            _comboInProgress = true;
            _actor.basicAttackController.OnAttackInput(input);
        }

        private void OnBasicAttackResolved(ComboResolution resolution)
        {
            if (resolution.kind == ComboResolutionKind.None)
            {
                return;
            }

            _actorExecutor.PlayAction(resolution.state, new ICombatTarget[] { targetSelector.CurrentTarget });
            _actorExecutor.ActionFinished += ReturnToIdleIfNothingQueued;

            if (resolution.kind == ComboResolutionKind.Finisher)
            {
                _comboInProgress = false;
                _comboUsedThisPhase = true;
            }
        }

        // Once this swing's animation ends, fall back to Idle unless the
        // player already queued the next swing (which Update()'s Tick will
        // consume and chain into on its own).
        private void ReturnToIdleIfNothingQueued()
        {
            _actorExecutor.ActionFinished -= ReturnToIdleIfNothingQueued;

            if (!_actor.basicAttackController.HasBufferedInput)
            {
                _actorExecutor.ReturnToIdle();
            }
        }

        private void ResetBasicAttackStateForNewPhase()
        {
            _comboInProgress = false;
            _comboUsedThisPhase = false;
        }
    }
}
