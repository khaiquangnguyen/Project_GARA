using System;

namespace GARA.Characters.Gambler
{
    // What a slot machine symbol pays out.
    [Serializable]
    public abstract class SlotReward
    {
        public abstract void Apply(in PassiveContext context);
    }
}
