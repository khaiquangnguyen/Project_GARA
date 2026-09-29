using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Each shot that fires hits the targets for baseDamage, less
    // damageLostPerWhiff for every whiff before it in the magazine.
    [Serializable]
    public class MagDumpShotDamage : GambleEffect<RouletteBarrageOutcome>
    {
        [Min(0)]
        public int baseDamage = 20;

        [Min(0)]
        public int damageLostPerWhiff = 3;

        [Min(0)]
        public int minDamage = 1;

        public override ISkillEffect ForStrike(int strike)
        {
            return new StrikeDamage(this, strike);
        }

        // Damage of shot (0-based) if it fires.
        public int DamageForShot(RouletteBarrageOutcome outcome, int shot)
        {
            var whiffs = 0;
            for (var i = 0; i < shot && i < outcome.Fired.Count; i++)
            {
                if (!outcome.Fired[i])
                {
                    whiffs++;
                }
            }

            return Mathf.Max(minDamage, baseDamage - whiffs * damageLostPerWhiff);
        }

        // Always: every shot plays, even when none fire.
        protected override bool IsTriggered(RouletteBarrageOutcome outcome, SkillPerformance performance)
        {
            return true;
        }

        protected override void ApplyEffect(in SkillEffectContext context, RouletteBarrageOutcome outcome)
        {
            Damage(context, baseDamage);
        }

        private static void Damage(in SkillEffectContext context, int damage)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }

        private sealed class StrikeDamage : ISkillEffect
        {
            private readonly MagDumpShotDamage _owner;
            private readonly int _strike;

            public StrikeDamage(MagDumpShotDamage owner, int strike)
            {
                _owner = owner;
                _strike = strike;
            }

            public void ApplyEffect(in SkillEffectContext context)
            {
                if (OutcomeOf(context) is RouletteBarrageOutcome outcome)
                {
                    Damage(context, _owner.DamageForShot(outcome, _strike));
                }
            }
        }
    }
}
