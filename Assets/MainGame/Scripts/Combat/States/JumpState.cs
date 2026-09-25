using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Plays its clip while lifting the character along JumpSpec's arc; always
    // lands back at the takeoff position.
    public class JumpState : NamedSpineAnimationState
    {
        private JumpSpec _spec;
        private Transform _characterRoot;
        private Vector3 _origin;
        private float _elapsed;
        private bool _isJumping;

        protected override bool FinishesOnAnimationComplete => false;

        public void SetSpec(JumpSpec spec)
        {
            _spec = spec;
        }

        public override void Enter(CharacterStateContext context)
        {
            base.Enter(context);

            if (_characterRoot == null)
            {
                var definition = GetComponentInParent<CharacterDefinition>();
                _characterRoot = definition != null ? definition.transform : transform.root;
            }

            _origin = _characterRoot.position;
            _elapsed = 0f;
            _isJumping = true;
        }

        public override void Exit()
        {
            base.Exit();
            Land();
        }

        private void Update()
        {
            if (!_isJumping)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            var duration = _spec != null ? _spec.JumpDurationSeconds : 0f;
            var t = duration <= 0f ? 1f : Mathf.Clamp01(_elapsed / duration);
            if (t >= 1f)
            {
                Land();
                RaiseFinished();
                return;
            }

            _characterRoot.position = _origin + Vector3.up * (_spec.JumpCurve.Evaluate(t) * _spec.JumpHeight);
        }

        private void Land()
        {
            if (!_isJumping)
            {
                return;
            }

            _isJumping = false;
            _characterRoot.position = _origin;
        }
    }
}
