using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The Gambler's skills deal more damage for a while.
    [Serializable]
    public class PowerUpSlotReward : SlotReward
    {
        [Min(0f)]
        public float damageIncrease = 0.2f;

        [Tooltip("Turns of the Gambler's own; 2 lasts through its next turn (statuses tick at turn start).")]
        [Min(1)]
        public int turns = 2;

        public override void Apply(in PassiveContext context)
        {
            context.Self.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Powered, turns, "slot machine") { outgoingDamageMultiplier = 1f + damageIncrease });
        }
    }
}
