using System;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Combat
{
    // Resolves a single character's attacks/specials into CharacterState
    // transitions once a turn controller has decided what to play. Owns the
    // "is this character still busy" gate itself — a state is busy from the
    // moment it's entered until it raises Finished, so a turn controller
    // can't act again until then. Lives on the battle prefab alongside
    // CharacterDefinition/CharacterStateMachine/CharacterStates.
    public class AttackExecutor : MonoBehaviour
    {
        [FormerlySerializedAs("_stateMachine")]
        [SerializeField]
        private CharacterStateMachine stateMachine;

        private CombatParticipant _participant;
        private IBattleQuery _battleQuery;
        private CharacterState _activeState;
        private bool _isBusy;

        public bool IsBusy => _isBusy;
        public CombatParticipant Participant => _participant;

        // Fired whenever the active state finishes, after busy-gating has
        // already been cleared — lets a turn controller decide what to do
        // next (chain into another action, or fall back to Idle).
        public event Action ActionFinished;

        public void Initialize(CombatParticipant participant, IBattleQuery battleQuery)
        {
            _participant = participant;
            _battleQuery = battleQuery;
            EnterDefaultState();
        }

        // Always Idle today — resolved the same way as any other authored
        // state (CharacterDefinition.defaultState), not hardcoded to a
        // specific concrete type, and deliberately skips the busy-gating
        // bookkeeping OnResolved uses: the default state isn't an attack,
        // so it shouldn't block input.
        private void EnterDefaultState()
        {
            var defaultState = _participant.ResolveLiveState(_participant.definition.defaultState);
            if (defaultState != null)
            {
                stateMachine.ChangeState(defaultState, new CharacterStateContext(_battleQuery, _participant));
            }
        }

        // Single entry point for playing any resolved action — a basic
        // attack/finisher or a special — against an explicit target list.
        // Callers resolve the asset-side CharacterState reference themselves
        // (ComboResolution.state, SpecialAttackEntry.state) and pass it here.
        public void PlayAction(CharacterState assetSideState, IReadOnlyList<ICombatTarget> targets)
        {
            var liveState = _participant.ResolveLiveState(assetSideState);
            if (liveState == null)
            {
                return; // misauthored prefab — missing component
            }

            if (_activeState != null)
            {
                _activeState.Finished -= OnActiveStateFinished;
            }

            _activeState = liveState;
            _isBusy = true;
            _activeState.Finished += OnActiveStateFinished;

            stateMachine.ChangeState(liveState, new CharacterStateContext(_battleQuery, _participant, targets));
        }

        // Re-enters the character's default (Idle) state — public so a turn
        // controller can fall back to it once an action finishes with
        // nothing queued up to chain into.
        public void ReturnToIdle()
        {
            EnterDefaultState();
        }

        private void OnActiveStateFinished()
        {
            _isBusy = false;

            if (_activeState != null)
            {
                _activeState.Finished -= OnActiveStateFinished;
                _activeState = null;
            }

            ActionFinished?.Invoke();
        }
    }
}
