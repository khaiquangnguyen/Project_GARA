using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Damage effect mirroring DamageSkillEffect's attacker/defender stat
    // scaling, but boosted when the recipe's designated "key step" (e.g. the
    // sear, the reduction) came out exquisite, and cut when anything burnt.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Chef/Effects/Key Step Damage")]
    public class KeyStepDamageSkillEffect : SkillEffectDefinition
    {
        [SerializeField]
        private int baseDamage;

        [SerializeField]
        private TierScaling tierMultiplier;

        [SerializeField]
        private StatScaling attackerScaling;

        [SerializeField]
        private StatScaling defenderScaling;

        [SerializeField]
        private float keyStepExquisiteMultiplier = 1.5f;

        [SerializeField]
        private float burntMultiplier = 0.5f;

        public override void Resolve(in SkillEffectContext context)
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
            var attackerOffensiveStatValue = attackerStats != null ? attackerStats.Attack.Value : 0;
            var attackFactor = attackerScaling.Evaluate(attackerOffensiveStatValue);

            foreach (var target in context.Targets)
            {
                var defenderOffensiveStatValue = target.CurrentStats.Defense.Value;
                var defenseFactor = defenderScaling.Evaluate(defenderOffensiveStatValue);

                var damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * tierMult * attackFactor - defenseFactor));
                target.ApplyDamage(damage);
            }
        }
    }
}
