using System;
using GARA.Characters;

namespace GARA.Combat
{
    [Serializable]
    public struct PlayerRosterSlot
    {
        public CharacterDefinition character;
        public ControlMode control;

        public bool IsPlayerControlled => control != ControlMode.Ai;
    }
}
