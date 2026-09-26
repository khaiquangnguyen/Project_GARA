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
        private ParryState _parryState;
        private JumpState _jumpState;
        private DeathState _deathState;
        private Vector3 _standardPosition;

        private CharacterState _activeState;
        private Action _activeStateFinishedHandler;
        private bool _isBusy;

        private CharacterState _pendingAttackState;
        private IReadOnlyList<ICombatTarget> _pendingTargets;
        private Action _pendingOnImpact;
        private Func<float> _pendingBeforeAttack;

        private readonly Dictionary<GameObject, IAttackHitFeedback> _hitFeedbacks = new();

        public bool IsBusy => _isBusy;
        public CombatParticipant Participant => _participant;

        // Once dead, the death state is final: no hit/parry/jump reaction,
        // idle or reposition may replace it.
        private bool IsDead => _participant != null && _participant.IsDefeated;

        public event Action ActionFinished;

        public void Initialize(CombatParticipant participant, IBattleQuery battleQuery)
        {
            _participant = participant;
            _battleQuery = battleQuery;
            _moveForwardState = _participant.SceneRoot.GetComponentInChildren<MoveForwardState>(true);
            _moveBackwardState = _participant.SceneRoot.GetComponentInChildren<MoveBackwardState>(true);
            _hitState = _participant.SceneRoot.GetComponentInChildren<HitState>(true);
            _parryState = _participant.SceneRoot.GetComponentInChildren<ParryState>(true);
            _jumpState = _participant.SceneRoot.GetComponentInChildren<JumpState>(true);
            _deathState = _participant.SceneRoot.GetComponentInChildren<DeathState>(true);
            _standardPosition = _participant.SceneTransform.position;
            EnterDefaultState();
        }

        private void EnterDefaultState()
        {
            if (IsDead)
            {
                return;
            }

            var defaultState = _participant.ResolveLiveState(_participant.definition.defaultState);
            if (defaultState != null)
            {
                stateMachine.ChangeState(defaultState, new CharacterStateContext(_battleQuery, _participant));
            }
        }

        // Plays one swing (a basic attack, a combo finisher, or a special)
        // against an explicit target list. positionMode is absolute: it
        // first dashes to the first target (MoveInFrontOfEnemy) or back to
        // the standard spot (StayAtOriginalPosition). Does not move the actor
        // back — a Combat Phase "Action" (a special, or the whole chained
        // basic-attack sequence) may be made of several of these calls, so
        // the caller decides when the Action is actually over and calls
        // ReturnToStandardPosition then.
        // onImpact (optional) is threaded through to the primary attack
        // state's CharacterStateContext.OnImpact — never to the move-in/
        // move-out states, since those aren't the "action" itself. Null by
        // default so existing callers are unaffected.
        // spec (optional) plays instead of the state's own spec, this time
        // only. The move-in range comes from whichever spec will play.
        // beforeAttack (optional) runs right before the attack state plays,
        // after any move-in; it returns seconds to hold the attack back.
        public void PlayAction(CharacterState assetSideState, IReadOnlyList<ICombatTarget> targets, ActionPositionMode positionMode, Action onImpact = null, AttackAnimationSpec spec = null, Func<float> beforeAttack = null)
        {
            var liveState = _participant.ResolveLiveState(assetSideState);
            if (liveState == null)
            {
                return; // misauthored prefab — missing component
            }

            if (spec != null && liveState is AttackSpecAnimationState attackState)
            {
                attackState.OverrideNextSpec(spec);
            }

            _isBusy = true;
            _pendingAttackState = liveState;
            _pendingTargets = targets;
            _pendingOnImpact = onImpact;
            _pendingBeforeAttack = beforeAttack;

            if (positionMode == ActionPositionMode.MoveInFrontOfEnemy
                && _moveForwardState != null
                && targets.Count > 0
                && targets[0] is CombatParticipant frontTarget)
            {
                var destination = CombatSpacing.PositionInFrontOfEnemy(_participant, frontTarget, RangeOf(liveState));
                MoveThenAttack(_moveForwardState, destination, targets);
            }
            else if (positionMode == ActionPositionMode.StayAtOriginalPosition && _moveBackwardState != null)
            {
                MoveThenAttack(_moveBackwardState, _standardPosition, targets);
            }
            else
            {
                BeginAttackState();
            }
        }

        private void MoveThenAttack(MoveState moveState, Vector3 destination, IReadOnlyList<ICombatTarget> targets)
        {
            if (Vector3.Distance(_participant.SceneTransform.position, destination) <= AlreadyAtDestinationDistance)
            {
                BeginAttackState();
                return;
            }

            moveState.SetDestination(destination);
            BeginState(moveState, targets, OnMoveForwardFinished);
        }

        // Seconds from an in-place PlayAction of assetSideState to its hit
        // event. False if the state has no hit event to time against.
        public bool TryGetSecondsToImpact(CharacterState assetSideState, out float seconds)
        {
            seconds = 0f;
            return _participant.ResolveLiveState(assetSideState) is AttackSpecAnimationState attackState
                   && attackState.TryGetSecondsToHit(out seconds);
        }

        public void ReturnToIdle()
        {
            EnterDefaultState();
        }

        // Plays a hit-feedback prefab on this character, spawning it under
        // SceneRoot the first time and reusing that instance after.
        public void PlayHitFeedback(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if (!_hitFeedbacks.TryGetValue(prefab, out var feedback))
            {
                feedback = Instantiate(prefab, _participant.SceneTransform).GetComponent<IAttackHitFeedback>();
                if (feedback == null)
                {
                    Debug.LogWarning($"[{nameof(AttackExecutor)}] {prefab.name} has no {nameof(IAttackHitFeedback)} at its root.", prefab);
                }

                _hitFeedbacks[prefab] = feedback;
            }

            feedback?.Play();
        }

        private static float RangeOf(CharacterState liveState)
        {
            if (liveState is AttackSpecAnimationState { NextSpec: { } spec })
            {
                return spec.Range;
            }

            Debug.LogWarning($"[{nameof(AttackExecutor)}] {liveState} has no {nameof(AttackAnimationSpec)} — moving in at range 0.", liveState);
            return 0f;
        }

        // Dashes up to target at assetSideState's attack range, or spec's
        // when given (as MoveInFrontOfEnemy does before a swing), idles
        // there, then calls onArrived. Calls straight back if there's nothing to dash for.
        public void MoveInFrontOf(ICombatTarget target, CharacterState assetSideState, Action onArrived, AttackAnimationSpec spec = null)
        {
            if (_moveForwardState == null || !(target is CombatParticipant enemy))
            {
                onArrived?.Invoke();
                return;
            }

            var range = spec != null ? spec.Range : RangeOf(_participant.ResolveLiveState(assetSideState));
            var destination = CombatSpacing.PositionInFrontOfEnemy(_participant, enemy, range);
            if (Vector3.Distance(_participant.SceneTransform.position, destination) <= AlreadyAtDestinationDistance)
            {
                onArrived?.Invoke();
                return;
            }

            _isBusy = true;
            _moveForwardState.SetDestination(destination);
            BeginState(_moveForwardState, Array.Empty<ICombatTarget>(), () =>
            {
                OnRepositionFinished();
                onArrived?.Invoke();
            });
        }

        // Briefly plays this character's HitState reaction, then falls
        // back to Idle — called whenever this character actually takes
        // damage (see CombatParticipant.ApplyDamage). Skipped if the
        // character is already busy doing something of its own (e.g. it's
        // the actor currently mid-swing) so a hit reaction never interrupts
        // that character's own action.
        public void PlayHitReaction()
        {
            if (_hitState == null || _isBusy || IsDead)
            {
                return;
            }

            BeginState(_hitState, Array.Empty<ICombatTarget>(), OnHitReactionFinished);
        }

        private void OnHitReactionFinished()
        {
            ReturnToIdle();
        }

        // Played on every Parry press, hit or miss.
        public void PlayParry()
        {
            if (_parryState == null || _isBusy || IsDead)
            {
                return;
            }

            BeginState(_parryState, Array.Empty<ICombatTarget>(), OnParryFinished);
        }

        private void OnParryFinished()
        {
            ReturnToIdle();
        }

        // Played on every Jump press, hit or miss.
        public void PlayJump(JumpSpec spec)
        {
            if (_jumpState == null || _isBusy || IsDead)
            {
                return;
            }

            _jumpState.SetSpec(spec);
            BeginState(_jumpState, Array.Empty<ICombatTarget>(), OnJumpFinished);
        }

        private void OnJumpFinished()
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
            if (_deathState == null || _activeState == _deathState)
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
            if (IsDead)
            {
                return;
            }

            if (_moveBackwardState == null)
            {
                ReturnToIdle();
                return;
            }

            _isBusy = true;
            _moveBackwardState.SetDestination(_standardPosition);
            BeginState(_moveBackwardState, Array.Empty<ICombatTarget>(), OnRepositionFinished);
        }

        private void OnRepositionFinished()
        {
            _isBusy = false;
            ReturnToIdle();
        }

        // Steps back to a retreat spot while an action this character isn't
        // targeted by plays out (see CombatSceneManager.Retreat), then idles
        // there. Busy for the duration of the move.
        public void RetreatTo(Vector3 destination)
        {
            MoveTo(_moveBackwardState, destination);
        }

        // Walks back from a retreat spot to the standard combat position.
        public void ReturnFromRetreat()
        {
            MoveTo(_moveForwardState, _standardPosition);
        }

        // Safety rail: puts the character straight back at its standard
        // combat position, cutting short any move in progress.
        public void SnapToStandardPosition()
        {
            _isBusy = false;
            _participant.SceneTransform.position = _standardPosition;
            ReturnToIdle();
        }

        private void MoveTo(MoveState moveState, Vector3 destination)
        {
            if (IsDead)
            {
                return;
            }

            if (moveState == null)
            {
                _participant.SceneTransform.position = destination;
                ReturnToIdle();
                return;
            }

            _isBusy = true;
            moveState.SetDestination(destination);
            BeginState(moveState, Array.Empty<ICombatTarget>(), OnRepositionFinished);
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
            var holdBack = _pendingBeforeAttack?.Invoke() ?? 0f;
            _pendingBeforeAttack = null;
            if (holdBack > 0f)
            {
                StartCoroutine(PlayPendingAttackStateAfter(holdBack));
                return;
            }

            PlayPendingAttackState();
        }

        private IEnumerator PlayPendingAttackStateAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            PlayPendingAttackState();
        }

        private void PlayPendingAttackState()
        {
            BeginState(_pendingAttackState, _pendingTargets, OnAttackStateFinished, _pendingOnImpact);
        }

        private void OnAttackStateFinished()
        {
            _isBusy = false;
            _pendingAttackState = null;
            _pendingTargets = null;
            _pendingOnImpact = null;
            _pendingBeforeAttack = null;
            ActionFinished?.Invoke();
        }

        private void BeginState(CharacterState state, IReadOnlyList<ICombatTarget> targets, Action onFinished, Action onImpact = null)
        {
            if (_activeState != null)
            {
                _activeState.Finished -= _activeStateFinishedHandler;
            }

            _activeState = state;
            _activeStateFinishedHandler = onFinished;
            _activeState.Finished += _activeStateFinishedHandler;

            stateMachine.ChangeState(_activeState, new CharacterStateContext(_battleQuery, _participant, targets, onImpact));
        }
    }
}
