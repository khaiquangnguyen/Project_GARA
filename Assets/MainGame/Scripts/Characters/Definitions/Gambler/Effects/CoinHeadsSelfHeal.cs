using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Heals the Gambler for each coin that landed heads.
    [Serializable]
    public class CoinHeadsSelfHeal : GambleEffect<CoinTossOutcome>
    {
        [Min(1)]
        public int healPerHeads = 8;

        protected override bool IsTriggered(CoinTossOutcome outcome, SkillPerformance performance)
        {
            return outcome.HeadsCount > 0;
        }

        protected override void ApplyEffect(in SkillEffectContext context, CoinTossOutcome outcome)
        {
            context.Self?.Heal(healPerHeads * outcome.HeadsCount);
        }
    }
}
