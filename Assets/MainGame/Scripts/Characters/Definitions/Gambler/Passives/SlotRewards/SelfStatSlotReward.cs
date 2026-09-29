using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    [Serializable]
    public class SelfStatSlotReward : SlotReward
    {
        public StatKind stat = StatKind.Defense;

        public float magnitude = 3f;

        [Min(1)]
        public int turns = 2;

        public override void Apply(in PassiveContext context)
        {
            context.Self.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, turns, "slot machine"));
        }
    }
}
