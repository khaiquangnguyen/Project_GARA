using System;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Which input maps to which attack state. All authoring for the attack
    // itself (animation, duration, on-hit behavior) lives on the referenced
    // CharacterState, a component on this character's own prefab.
    [Serializable]
    public struct BasicAttackEntry
    {

        public AttackInput input;

        public CharacterState state;

        public int apCost;
    }
}
