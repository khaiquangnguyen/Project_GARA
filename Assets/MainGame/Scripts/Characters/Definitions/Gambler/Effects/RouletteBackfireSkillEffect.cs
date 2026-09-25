using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Applies the Russian Roulette game's backfire damage (a fraction of the
    // caster's own max HP) back onto the caster when the outcome carries one.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Effects/Roulette Backfire")]
    public class RouletteBackfireSkillEffect : GambleAwareSkillEffect
    {
        public override void Resolve(in SkillEffectContext context)
        {
            if (!TryGetResolution(context.Performance, out var resolution) || resolution.Outcome.BackfireDamageFraction <= 0f)
            {
                return;
            }

            var maxHp = context.Self.CurrentStats.MaxHp.Value;
            var damage = Mathf.RoundToInt(maxHp * resolution.Outcome.BackfireDamageFraction);
            context.Self.ApplyDamage(damage);
        }
    }
}
