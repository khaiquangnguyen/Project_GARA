using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Filmmaker
{
    // Flat damage to every target, only when the step's code is cleared.
    [Serializable]
    public class ClearedForDamage : InputSetStepEffect
    {
        [Min(1)]
        public int damage = 8;

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
