using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Stat-scaled damage; up on an exquisite key step, down if anything burnt.
    [Serializable]
    public class KeyStepDamage : InputSetStepEffect
    {
        public int baseDamage;

        public TierScaling tierMultiplier;

        public StatScaling attackerScaling;

        public StatScaling defenderScaling;

        public float keyStepExquisiteMultiplier = 1.5f;

        public float burntMultiplier = 0.5f;

        public override bool IsTriggered(InputSetCompletionReport report)
        {
            return DishCleared.AnyStep(report);
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            var tierMult = tierMultiplier.For(context.Performance.Tier);

            if (context.Performance.TryGetDetails<DishReport>(out var dish))
            {
                if (dish.HasKeyStep && dish.KeyStep.Quality == StepQuality.Exquisite)
                {
                    tierMult *= keyStepExquisiteMultiplier;
                }

                if (dish.AnyBurnt)
                {
                    tierMult *= burntMultiplier;
                }
            }

            var attackerStats = context.Self?.CurrentStats;
            var attackFactor = attackerScaling.Evaluate(attackerStats != null ? attackerStats.Attack.Value : 0);

            foreach (var target in context.Targets)
            {
                var defenseFactor = defenderScaling.Evaluate(target.CurrentStats.Defense.Value);
                var damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * tierMult * attackFactor - defenseFactor));
                target.ApplyDamage(damage);
            }
        }
    }
}
