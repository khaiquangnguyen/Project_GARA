using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Losing face: the seat that chickened out takes a flat hit.
    [Serializable]
    public class DuelRouletteChickenOutPenalty : DuelRouletteEffect
    {
        public DuelSeat seat = DuelSeat.Gambler;

        [Min(0)]
        public int damage = 8;

        protected override bool IsTriggered(DuelRouletteOutcome outcome, SkillPerformance performance)
        {
            return outcome.Ending == DuelEnding.ChickenedOut && outcome.EndedBy == seat;
        }

        protected override void ApplyEffect(in SkillEffectContext context, DuelRouletteOutcome outcome)
        {
            Damage(context, seat, damage);
        }
    }
}
