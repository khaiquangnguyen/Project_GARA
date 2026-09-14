using System;
using GARA.Characters;

namespace GARA.Combat
{
    // Real-time input handling for one character's turn: an ATK1/ATK2/ATK3
    // press is buffered (not dropped) while a previous attack is still
    // playing, then consumed once that attack ends and checked against the
    // character's combo definitions. Only the most recent press is kept —
    // an older buffered input is overwritten, not queued behind the new one.
    // The buffer never expires on its own — it waits indefinitely for
    // canAct to become true. Whether an attack is still playing is not this
    // class's concern — the caller passes that in as canAct (see
    // AttackExecutor, which tracks it off the live CharacterState's Finished
    // event).
    public class BasicAttackController
    {
        private readonly ComboTracker _combo;

        private bool _hasBufferedInput;
        private AttackInput _bufferedInput;

        public BasicAttackController(CharacterDefinition definition)
        {
            _combo = new ComboTracker(definition);
        }

        public void OnAttackInput(AttackInput input)
        {
            _hasBufferedInput = true;
            _bufferedInput = input;
        }

        // Call once per frame (or tick). Consumes the buffered input only
        // once canAct is true. Fires onResolved at most once per call, when
        // the buffered input is consumed.
        public void Tick(bool canAct, Action<ComboResolution> onResolved)
        {
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

        public bool TryTakeQueuedFinisher(out ComboResolution finisher) => _combo.TryTakeQueuedFinisher(out finisher);
    }
}
