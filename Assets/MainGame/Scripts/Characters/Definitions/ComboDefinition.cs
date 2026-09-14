using System;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Authored per character: a specific tail sequence of basic attack
    // inputs that, once matched, fires FinisherState instead of the last
    // input's own basic attack.
    [Serializable]
    public class ComboDefinition
    {
        public AttackInput[] sequence = Array.Empty<AttackInput>();

        public CharacterState finisherState;
    }
}
