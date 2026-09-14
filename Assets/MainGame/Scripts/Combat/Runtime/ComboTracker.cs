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

        public ComboTracker(CharacterDefinition definition)
        {
            _definition = definition;
        }

        public void Reset()
        {
            _sequence.Clear();
        }

        public ComboResolution Register(AttackInput input)
        {
            _sequence.Add(input);

            foreach (var combo in _definition.combos)
            {
                if (SequenceEndsWith(combo.sequence))
                {
                    Reset();
                    return ComboResolution.Finisher(combo.finisherState);
                }
            }

            return _definition.TryGetBasicAttack(input, out var basicAttack)
                ? ComboResolution.Basic(basicAttack.state)
                : ComboResolution.None();
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
