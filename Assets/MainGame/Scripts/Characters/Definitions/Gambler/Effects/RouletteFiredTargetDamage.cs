using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The chamber was loaded: every target takes the shot.
    [Serializable]
    public class RouletteFiredTargetDamage : GambleEffect<RussianRouletteOutcome>
    {
        [Min(1)]
        public int damage = 45;

        protected override bool IsTriggered(RussianRouletteOutcome outcome, SkillPerformance performance)
        {
            return outcome.Fired;
        }

        protected override void ApplyEffect(in SkillEffectContext context, RussianRouletteOutcome outcome)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
