using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Timed buff/debuff sized by how many recipe steps came out well, rather
    // than by the generic 0-1 performance score alone — reads DishReport
    // back out of SkillPerformance.Details when available (a RecipeSkillCard
    // ran), and falls back to a score-based approximation for any other
    // card that happens to reference this effect.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Chef/Effects/Dish Stat Modifier")]
    public class DishStatModifierSkillEffect : SkillEffectDefinition
    {
        [SerializeField]
        private StatKind stat;

        [SerializeField]
        private float magnitudePerStep;

        [SerializeField]
        private float exquisiteStepBonus;

        [SerializeField]
        private int baseDurationTurns = 1;

        [SerializeField]
        private TierScaling durationScaling;

        [SerializeField]
        private bool applyToSelfInsteadOfTargets;

        [SerializeField]
        private bool halveIfAnyBurnt;

        public override void Resolve(in SkillEffectContext context)
        {
            var duration = baseDurationTurns + Mathf.RoundToInt(durationScaling.For(context.Performance.Tier));
            float magnitude;

            if (context.Performance.TryGetDetails<DishReport>(out var dish))
            {
                magnitude = 0f;

                foreach (var step in dish.Steps)
                {
                    if (step.Quality == StepQuality.Ruined)
                    {
                        continue;
                    }

                    magnitude += magnitudePerStep;

                    if (step.Quality == StepQuality.Exquisite)
                    {
                        magnitude += exquisiteStepBonus;
                    }
                }

                if (halveIfAnyBurnt && dish.AnyBurnt)
                {
                    magnitude *= 0.5f;
                }
            }
            else
            {
                magnitude = magnitudePerStep * context.Performance.Score * 4f;
            }

            var modifier = new TimedStatModifier(stat, magnitude, duration, name);

            if (applyToSelfInsteadOfTargets)
            {
                context.Self?.ApplyTimedModifier(modifier);
                return;
            }

            foreach (var target in context.Targets)
            {
                target.ApplyTimedModifier(modifier);
            }
        }
    }
}
