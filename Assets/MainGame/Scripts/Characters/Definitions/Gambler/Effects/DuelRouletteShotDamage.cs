using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The seat that got shot takes base damage plus a bit per empty click
    // before it.
    [Serializable]
    public class DuelRouletteShotDamage : DuelRouletteEffect
    {
        public DuelSeat seat = DuelSeat.Target;

        [Min(0)]
        public int baseDamage = 10;

        [Min(0)]
        public int damagePerClick = 5;

        protected override bool IsTriggered(DuelRouletteOutcome outcome, SkillPerformance performance)
        {
            return outcome.Ending == DuelEnding.Shot && outcome.EndedBy == seat;
        }

        protected override void ApplyEffect(in SkillEffectContext context, DuelRouletteOutcome outcome)
        {
            Damage(context, seat, baseDamage + damagePerClick * outcome.EmptyClicks);
        }
    }
}
