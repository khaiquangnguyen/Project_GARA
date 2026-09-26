using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Timed buff/debuff sized by how many steps came out well.
    [Serializable]
    public class DishStatModifier : InputSetStepEffect
    {
        public StatKind stat;

        public float magnitudePerStep;

        public float exquisiteStepBonus;

        public int baseDurationTurns = 1;

        public TierScaling durationScaling;

        public bool applyToSelfInsteadOfTargets;

        public bool halveIfAnyBurnt;

        public override bool IsTriggered(InputSetCompletionReport report)
        {
            return DishCleared.AnyStep(report);
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            var duration = baseDurationTurns + Mathf.RoundToInt(durationScaling.For(context.Performance.Tier));
            var magnitude = 0f;

            if (context.Performance.TryGetDetails<DishReport>(out var dish))
            {
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

            var modifier = new TimedStatModifier(stat, magnitude, duration, nameof(DishStatModifier));

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
