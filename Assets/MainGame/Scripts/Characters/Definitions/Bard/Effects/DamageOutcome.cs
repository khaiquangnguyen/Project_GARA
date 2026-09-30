using System;
using UnityEngine;

namespace GARA.Characters.Bard
{
    [Serializable]
    public class DamageOutcome : EmotionOutcome
    {
        [Min(1)]
        public int damage = 10;

        public OutcomeTargets targets = OutcomeTargets.CardTargets;

        [Tooltip("RandomEnemies only: how many different enemies are hit.")]
        [Min(1)]
        public int randomCount = 3;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in TargetsOf(in context, targets, randomCount))
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
