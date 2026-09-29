using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    [Serializable]
    public class DamageSlotReward : SlotReward
    {
        [Min(1)]
        public int damage = 6;

        public SpecialTargetMode targetMode = SpecialTargetMode.AllEnemy;

        [Tooltip("Targets picked for the Multi* modes.")]
        [Min(1)]
        public int multiTargetCount = 1;

        public override void Apply(in PassiveContext context)
        {
            foreach (var target in PassiveEffectRunner.ResolveTargets(context.Battle, context.Self, targetMode, multiTargetCount))
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
