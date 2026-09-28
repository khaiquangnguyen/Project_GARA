using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Every target takes the dice total times damagePerPip.
    [Serializable]
    public class DiceTotalDamage : GambleEffect<DiceOutcome>
    {
        [Min(0)]
        public int damagePerPip = 3;

        protected override bool IsTriggered(DiceOutcome outcome, SkillPerformance performance)
        {
            return true;
        }

        protected override void ApplyEffect(in SkillEffectContext context, DiceOutcome outcome)
        {
            var damage = outcome.Total * damagePerPip;
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
