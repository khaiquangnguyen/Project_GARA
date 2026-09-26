using System;

namespace GARA.Combat
{
    [Serializable]
    public struct EnemyRosterSlot
    {
        public EnemyEncounterData encounter;
        public ControlMode control;

        // Fights for the players' side from its enemy slot, player-controlled.
        public bool charmedToPlayerSide;

        public bool IsPlayerControlled => control == ControlMode.Player;
    }
}
