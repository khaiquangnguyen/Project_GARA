using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Flat damage to every target when every set in the report is cleared.
    [Serializable]
    public class AllSetsClearedForDamage : InputSetStepEffect
    {
        [Min(1)]
        public int damage = 10;

        public override bool IsTriggered(InputSetCompletionReport report)
        {
            return report.TotalSets > 0 && report.ClearedSets == report.TotalSets;
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
