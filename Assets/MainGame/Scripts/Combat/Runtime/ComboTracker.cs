using System.Collections.Generic;
using GARA.Characters;

namespace GARA.Combat
{
    // Tracks the running sequence of basic attack inputs for one character's
    // current turn and matches it against that character's authored combos.
    // A combo only needs to match the tail of the sequence, so an earlier
    // whiffed input doesn't permanently block a combo later in the same turn.
    public class ComboTracker
    {
        private readonly CharacterDefinition _definition;
        private readonly List<AttackInput> _sequence = new();

        private bool _hasQueuedFinisher;
        private ComboResolution _queuedFinisher;

        public ComboTracker(CharacterDefinition definition)
        {
            _definition = definition;
        }

        public void Reset()
        {
            _sequence.Clear();
            _hasQueuedFinisher = false;
        }

        // Registers the input's own basic attack as the resolution — a
        // matched combo tail never bypasses the final input's own swing,
        // it just queues the finisher to play right after that swing
        // finishes (see TryTakeQueuedFinisher).
        public ComboResolution Register(AttackInput input)
        {
            _sequence.Add(input);

            foreach (var combo in _definition.combos)
            {
                if (SequenceEndsWith(combo.sequence))
                {
                    _hasQueuedFinisher = true;
                    _queuedFinisher = ComboResolution.Finisher(combo.finisherState, combo.finisherState.PositionMode);
                    _sequence.Clear();
                    break;
                }
            }

            return _definition.TryGetBasicAttack(input, out var basicAttack)
                ? ComboResolution.Basic(basicAttack.state, basicAttack.positionMode)
                : ComboResolution.None();
        }

        public bool TryTakeQueuedFinisher(out ComboResolution finisher)
        {
            if (!_hasQueuedFinisher)
            {
                finisher = ComboResolution.None();
                return false;
            }

            finisher = _queuedFinisher;
            _hasQueuedFinisher = false;
            return true;
        }

        private bool SequenceEndsWith(AttackInput[] comboSequence)
        {
            if (comboSequence.Length == 0 || _sequence.Count < comboSequence.Length)
            {
                return false;
            }

            var offset = _sequence.Count - comboSequence.Length;
            for (var i = 0; i < comboSequence.Length; i++)
            {
                if (_sequence[offset + i] != comboSequence[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
