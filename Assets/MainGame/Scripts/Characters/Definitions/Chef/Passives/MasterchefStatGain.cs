using System;

namespace GARA.Characters.Chef
{
    // One stat a Masterchef stack can raise, for the rest of the battle.
    [Serializable]
    public struct MasterchefStatGain
    {
        public StatKind stat;
        public float amount;
    }
}
