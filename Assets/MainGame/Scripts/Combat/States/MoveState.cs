using GARA.Characters;
using UnityEngine;
using UnityEngine.Events;

namespace GARA.Combat
{
    // Shared "move to a destination over a fixed duration" behavior for
    // MoveForwardState/MoveBackwardState — a placeholder until real
    // walk/dash Spine animations exist. AttackExecutor sets the destination
    // via SetDestination immediately before entering this state (Enter's
    // CharacterStateContext has no notion of a world position to move to).
    // X eases along its own normalized (0-1 in, 0-1 out) curve toward the
    // destination. Y has no real distance to travel for a horizontal move
    // (origin.y == destination.y), so curveY isn't an easing curve at all —
    // it's a vertical OFFSET added on top of the linear origin→destination
    // baseline, scaled by yOffsetAmount (e.g. a hop arc: a curve shaped
    // 0→1→0 with yOffsetAmount as the hop height). Z stays a plain linear
    // lerp. onBeginMove is a UnityEvent (not a direct MMF_Player reference
    // — GARA.Combat's asmdef can't reference MoreMountains.Feedbacks, which
    // lives in the default assembly) wired in the Inspector to whatever
    // feedback should play, if any, the instant the move starts.
    public abstract class MoveState : CharacterState
    {
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private AnimationCurve curveX = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve curveY = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private float yOffsetAmount;
        [SerializeField] private UnityEvent onBeginMove;

        private Vector3 _origin;
        private Vector3 _destination;
        private float _elapsed;
        private bool _isMoving;

        public void SetDestination(Vector3 worldPosition)
        {
            _destination = worldPosition;
        }

        public override void Enter(CharacterStateContext context)
        {
            _origin = transform.position;
            _elapsed = 0f;
            _isMoving = true;
            onBeginMove?.Invoke();
        }

        public override void Exit()
        {
            _isMoving = false;
        }

        private void Update()
        {
            if (!_isMoving)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            var t = duration <= 0f ? 1f : Mathf.Clamp01(_elapsed / duration);

            transform.position = new Vector3(
                Mathf.LerpUnclamped(_origin.x, _destination.x, curveX.Evaluate(t)),
                Mathf.Lerp(_origin.y, _destination.y, t) + curveY.Evaluate(t) * yOffsetAmount,
                Mathf.Lerp(_origin.z, _destination.z, t));

            if (t >= 1f)
            {
                _isMoving = false;
                RaiseFinished();
            }
        }
    }
}
