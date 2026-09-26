using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Damage to every target scaled by the roll's significance; any game.
    [Serializable]
    public class SignificanceScaledDamage : GambleEffect
    {
        [Range(0, 1)]
        public float minSignificance;

        [Min(0)]
        public int minDamage = 5;

        [Min(0)]
        public int maxDamage = 20;

        public override bool IsTriggered(GambleOutcome outcome, SkillPerformance performance)
        {
            return outcome != null && outcome.Significance >= minSignificance;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            var outcome = OutcomeOf(context);
            if (outcome == null)
            {
                return;
            }

            var damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, outcome.Significance));
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
