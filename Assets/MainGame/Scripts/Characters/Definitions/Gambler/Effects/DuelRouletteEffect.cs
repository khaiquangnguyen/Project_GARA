using System;
using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // An effect on how a Duel Roulette round ended.
    [Serializable]
    public abstract class DuelRouletteEffect : GambleEffect<DuelRouletteOutcome>
    {
        protected static void Damage(in SkillEffectContext context, DuelSeat seat, int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            if (seat == DuelSeat.Gambler)
            {
                context.Self?.ApplyDamage(damage);
                return;
            }

            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
