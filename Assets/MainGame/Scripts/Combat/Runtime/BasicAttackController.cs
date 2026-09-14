using System;
using GARA.Characters;

namespace GARA.Combat
{
    // Real-time input handling for one character's turn: an ATK1/ATK2/ATK3
    // press is buffered (not dropped) while a previous attack is still
    // playing, then consumed once that attack ends and checked against the
    // character's combo definitions. Only the most recent press is kept —
    // an older buffered input is overwritten, not queued behind the new one.
    // Ages the buffered input by counting down a timer each Tick rather than
    // comparing against a captured "current time" — callers only ever supply
    // a deltaTime, never a clock. Whether an attack is still playing is not
    // this class's concern — the caller passes that in as canAct (see
    // AttackExecutor, which tracks it off the live CharacterState's Finished
    // event).
    public class BasicAttackController
    {
        private const float BufferWindow = 0.2f;

        private readonly ComboTracker _combo;

        private bool _hasBufferedInput;
        private AttackInput _bufferedInput;
        private float _bufferTimeRemaining;

        // Whether a press is still waiting to be consumed — lets a caller
        // decide whether to chain into the next swing or fall back to Idle
        // once the current one finishes.
        public bool HasBufferedInput => _hasBufferedInput;

        public BasicAttackController(CharacterDefinition definition)
        {
            _combo = new ComboTracker(definition);
        }

        public void OnAttackInput(AttackInput input)
        {
            _hasBufferedInput = true;
            _bufferedInput = input;
            _bufferTimeRemaining = BufferWindow;
        }

        // Call once per frame (or tick) with the elapsed time since the last
        // call. The buffer ages down regardless of canAct, but nothing is
        // consumed from it until canAct is true. Fires onResolved at most
        // once per call, when the buffered input is consumed.
        public void Tick(float deltaTime, bool canAct, Action<ComboResolution> onResolved)
        {
            if (_hasBufferedInput)
            {
                _bufferTimeRemaining -= deltaTime;
                if (_bufferTimeRemaining <= 0f)
                {
                    _hasBufferedInput = false;
                }
            }

            if (!canAct || !_hasBufferedInput)
            {
                return;
            }

            _hasBufferedInput = false;

            var resolution = _combo.Register(_bufferedInput);
            if (resolution.kind == ComboResolutionKind.None)
            {
                return;
            }

            onResolved(resolution);
        }

        public void ResetForNewTurn()
        {
            _hasBufferedInput = false;
            _combo.Reset();
        }
    }
}
