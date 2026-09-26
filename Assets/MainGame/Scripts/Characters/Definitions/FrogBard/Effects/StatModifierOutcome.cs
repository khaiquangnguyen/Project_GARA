using System;
using UnityEngine;

namespace GARA.Characters.FrogBard
{
    // A flat timed buff (positive) or debuff (negative).
    [Serializable]
    public class StatModifierOutcome : EmotionOutcome
    {
        public StatKind stat = StatKind.Attack;

        public float amount = 5f;

        [Min(1)]
        public int turns = 2;

        public bool onSelfInsteadOfTargets;

        public override void Apply(in SkillEffectContext context)
        {
            const string source = "frog_bard";
            if (onSelfInsteadOfTargets)
            {
                context.Self?.ApplyTimedModifier(new TimedStatModifier(stat, amount, turns, source));
                return;
            }

            foreach (var target in context.Targets)
            {
                target.ApplyTimedModifier(new TimedStatModifier(stat, amount, turns, source));
            }
        }
    }
}
