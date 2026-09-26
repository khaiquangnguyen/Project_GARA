using System;
using UnityEngine;

namespace GARA.Characters.FrogBard
{
    [Serializable]
    public class DamageOutcome : EmotionOutcome
    {
        [Min(1)]
        public int damage = 10;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
