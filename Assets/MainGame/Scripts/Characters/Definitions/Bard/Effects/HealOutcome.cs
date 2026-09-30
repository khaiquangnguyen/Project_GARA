using System;
using UnityEngine;

namespace GARA.Characters.Bard
{
    [Serializable]
    public class HealOutcome : EmotionOutcome
    {
        [Min(1)]
        public int heal = 10;

        public OutcomeTargets targets = OutcomeTargets.CardTargets;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in TargetsOf(in context, targets))
            {
                target.Heal(heal);
            }
        }
    }
}
