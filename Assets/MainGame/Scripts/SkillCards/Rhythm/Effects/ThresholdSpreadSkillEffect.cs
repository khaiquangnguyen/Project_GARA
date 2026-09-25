using System.Collections.Generic;
using GARA.Characters;
using GARA.Rhythm;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // Runs innerEffect against the full target list once completion clears
    // spreadThreshold, otherwise narrows it to a single target - rewarding a
    // clean run with spread damage/effects and punishing a messy one by
    // concentrating it on the first target only.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Rhythm/Threshold Spread")]
    public class ThresholdSpreadSkillEffect : SkillEffectDefinition
    {
        [SerializeField, Range(0, 1)]
        private float spreadThreshold = 0.75f;

        [SerializeField]
        [Expandable]
        private SkillEffectDefinition innerEffect;

        public override void Resolve(in SkillEffectContext context)
        {
            if (innerEffect == null)
            {
                return;
            }

            var completionRate = context.Performance.TryGetDetails<RhythmCompletionReport>(out var report)
                ? report.CompletionRate
                : context.Performance.Score;

            if (completionRate >= spreadThreshold || context.Targets.Count == 0)
            {
                innerEffect.Resolve(context);
                return;
            }

            var singleTarget = new List<ICombatTarget> { context.Targets[0] };
            var singleContext = new SkillEffectContext(context.Battle, context.Self, singleTarget, context.Performance);
            innerEffect.Resolve(singleContext);
        }
    }
}
