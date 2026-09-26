using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The chamber was loaded: the Gambler shoots themself.
    [Serializable]
    public class RouletteFiredSelfDamage : GambleEffect<RussianRouletteOutcome>
    {
        [Min(1)]
        public int damage = 10;

        protected override bool IsTriggered(RussianRouletteOutcome outcome, SkillPerformance performance)
        {
            return outcome.Fired;
        }

        protected override void ApplyEffect(in SkillEffectContext context, RussianRouletteOutcome outcome)
        {
            context.Self?.ApplyDamage(damage);
        }
    }
}
