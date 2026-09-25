using UnityEngine;

namespace GARA.Characters
{
    // Applies a timed buff/debuff to self or every target, sized by
    // performance score (via magnitudeScaling, reusing StatScaling as a
    // generic score-to-magnitude curve) and lasting a duration derived from
    // performance tier.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Timed Stat Modifier", fileName = "TimedStatModifierSkillEffect")]
    public class TimedStatModifierSkillEffect : SkillEffectDefinition
    {
        public StatKind stat;

        public StatScaling magnitudeScaling;

        public int baseDurationTurns;

        public TierScaling durationScaling;

        public bool applyToSelfInsteadOfTargets;

        public override void Resolve(in SkillEffectContext context)
        {
            var magnitude = magnitudeScaling.Evaluate(context.Performance.Score);
            var duration = baseDurationTurns + Mathf.RoundToInt(durationScaling.For(context.Performance.Tier));

            if (applyToSelfInsteadOfTargets)
            {
                context.Self?.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, duration, name));
                return;
            }

            foreach (var target in context.Targets)
            {
                target.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, duration, name));
            }
        }
    }
}
