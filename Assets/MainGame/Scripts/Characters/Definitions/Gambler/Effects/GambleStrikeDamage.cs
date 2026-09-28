using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Flat damage to the targets on every strike; any game.
    [Serializable]
    public class GambleStrikeDamage : GambleEffect
    {
        [Min(0)]
        public int damage = 8;

        public override bool IsTriggered(GambleOutcome outcome, SkillPerformance performance)
        {
            return outcome != null;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
