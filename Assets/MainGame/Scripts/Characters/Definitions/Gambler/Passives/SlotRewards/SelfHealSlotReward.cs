using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    [Serializable]
    public class SelfHealSlotReward : SlotReward
    {
        [Min(1)]
        public int amount = 8;

        public override void Apply(in PassiveContext context)
        {
            context.Self.Heal(amount);
        }
    }
}
