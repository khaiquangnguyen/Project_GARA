using System;
using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Combat
{
    public class AttackExecutor : MonoBehaviour
    {
        [FormerlySerializedAs("_stateMachine")]
        [SerializeField]
        private CharacterStateMachine stateMachine;

        [Tooltip("Pause after reaching the target (MoveInFrontOfEnemy) before the attack state actually plays, so the swing doesn't fire the instant the dash-in finishes.")]
        [SerializeField]
        private float timeBetweenMoveAndAction = 0.2f;

        [Tooltip("Pause after the death animation finishes before the character's GameObject is actually deactivated.")]
        [SerializeField]
        private float deathDisappearDelay = 0.5f;

        // If the actor is already standing within this distance of where
        // MoveInFrontOfEnemy would send it, the whole move-forward step
        // (its curveY hop included, and the pause above) is skipped
        // entirely — there's nothing to dash for.
        private const float AlreadyAtDestinationDistance = 0.01f;

        private CombatParticipant _participant;
        private IBattleQuery _battleQuery;
        private MoveForwardState _moveForwardState;
        private MoveBackwardState _moveBackwardState;
        private HitState _hitState;
        private DeathState _deathState;
        private Vector3 _standardPosition;

        private CharacterState _activeState;
        private Action _activeStateFinishedHandler;
        private bool _isBusy;

        private CharacterState _pendingAttackState;
        private IReadOnlyList<ICombatTarget> _pendingTargets;

        public bool IsBusy => _isBusy;
        public CombatParticipant Participant => _participant;

        public event Action ActionFinished;

        public void Initialize(CombatParticipant participant, IBattleQuery battleQuery)
        {
            _participant = participant;
            _battleQuery = battleQuery;
            _moveForwardState = _participant.SceneRoot.GetComponentInChildren<MoveForwardState>(true);
            _moveBackwardState = _participant.SceneRoot.GetComponentInChildren<MoveBackwardState>(true);
            _hitState = _participant.SceneRoot.GetComponentInChildren<HitState>(true);
            _deathState = _participant.SceneRoot.GetComponentInChildren<DeathState>(true);
            _standardPosition = _participant.SceneTransform.position;
            EnterDefaultState();
        }

        private void EnterDefaultState()
        {
            var defaultState = _participant.ResolveLiveState(_participant.definition.defaultState);
            if (defaultState != null)
            {
                stateMachine.ChangeState(defaultState, new CharacterStateContext(_battleQuery, _participant));
            }
        }

        // Plays one swing (a basic attack, a combo finisher, or a special)
        // against an explicit target list, dashing to the first target
        // first when positionMode calls for it. Does not move the actor
        // back — a Combat Phase "Action" (a special, or the whole chained
        // basic-attack sequence) may be made of several of these calls, so
        // the caller decides when the Action is actually over and calls
        // ReturnToStandardPosition then.
        public void PlayAction(CharacterState assetSideState, IReadOnlyList<ICombatTarget> targets, ActionPositionMode positionMode)
        {
            var liveState = _participant.ResolveLiveState(assetSideState);
            if (liveState == null)
            {
                return; // misauthored prefab — missing component
            }

            _isBusy = true;
            _pendingAttackState = liveState;
            _pendingTargets = targets;

            if (positionMode == ActionPositionMode.MoveInFrontOfEnemy
                && _moveForwardState != null
                && targets.Count > 0
                && targets[0] is CombatParticipant frontTarget)
            {
                var destination = CombatSpacing.PositionInFrontOfEnemy(_participant, frontTarget);
                if (Vector3.Distance(_participant.SceneTransform.position, destination) > AlreadyAtDestinationDistance)
                {
                    _moveForwardState.SetDestination(destination);
                    BeginState(_moveForwardState, targets, OnMoveForwardFinished);
                }
                else
                {
                    BeginAttackState();
                }
            }
            else
            {
                BeginAttackState();
            }
        }

        public void ReturnToIdle()
        {
            EnterDefaultState();
        }

        // Briefly plays this character's HitState reaction, then falls
        // back to Idle — called whenever this character actually takes
        // damage (see CombatParticipant.ApplyDamage). Skipped if the
        // character is already busy doing something of its own (e.g. it's
        // the actor currently mid-swing) so a hit reaction never interrupts
        // that character's own action.
        public void PlayHitReaction()
        {
            if (_hitState == null || _isBusy)
            {
                return;
            }

            BeginState(_hitState, Array.Empty<ICombatTarget>(), OnHitReactionFinished);
        }

        private void OnHitReactionFinished()
        {
            ReturnToIdle();
        }

        // Plays this character's death animation, then — once it's
        // actually done playing — waits deathDisappearDelay and deactivates
        // the character's GameObject. Called once (see
        // CombatParticipant.ApplyDamage) the killing blow lands; takes
        // priority over a hit reaction, and isn't gated by _isBusy — dying
        // always overrides whatever the character was doing.
        public void PlayDeathReaction()
        {
            if (_deathState == null)
            {
                return;
            }

            BeginState(_deathState, Array.Empty<ICombatTarget>(), OnDeathAnimationFinished);
        }

        private void OnDeathAnimationFinished()
        {
            StartCoroutine(DisappearAfterDelay());
        }

        private IEnumerator DisappearAfterDelay()
        {
            yield return new WaitForSeconds(deathDisappearDelay);
            gameObject.SetActive(false);
        }

        // Dashes back to the actor's standard combat position — call once
        // a whole Action (a special, or the entire chained basic-attack
        // sequence including its finisher) is done.
        public void ReturnToStandardPosition()
        {
            if (_moveBackwardState == null)
            {
                ReturnToIdle();
                return;
            }

            _isBusy = true;
            _moveBackwardState.SetDestination(_standardPosition);
            BeginState(_moveBackwardState, Array.Empty<ICombatTarget>(), OnReturnToStandardPositionFinished);
        }

        private void OnReturnToStandardPositionFinished()
        {
            _isBusy = false;
            ReturnToIdle();
        }

        private void OnMoveForwardFinished()
        {
            StartCoroutine(BeginAttackStateAfterDelay());
        }

        private IEnumerator BeginAttackStateAfterDelay()
        {
            yield return new WaitForSeconds(timeBetweenMoveAndAction);
            BeginAttackState();
        }

        private void BeginAttackState()
        {
            BeginState(_pendingAttackState, _pendingTargets, OnAttackStateFinished);
        }

        private void OnAttackStateFinished()
        {
            _isBusy = false;
            _pendingAttackState = null;
            _pendingTargets = null;
            ActionFinished?.Invoke();
        }

        private void BeginState(CharacterState state, IReadOnlyList<ICombatTarget> targets, Action onFinished)
        {
            if (_activeState != null)
            {
                _activeState.Finished -= _activeStateFinishedHandler;
            }

            _activeState = state;
            _activeStateFinishedHandler = onFinished;
            _activeState.Finished += _activeStateFinishedHandler;

            stateMachine.ChangeState(_activeState, new CharacterStateContext(_battleQuery, _participant, targets));
        }
    }
}
