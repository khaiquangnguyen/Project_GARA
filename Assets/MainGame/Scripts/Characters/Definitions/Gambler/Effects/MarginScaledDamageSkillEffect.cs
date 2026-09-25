using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Tier-based base damage (per the standard TierScaling pattern), boosted
    // by how decisive the gamble's margin was when a GambleResolution is
    // available.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Effects/Margin Scaled Damage")]
    public class MarginScaledDamageSkillEffect : GambleAwareSkillEffect
    {
        [SerializeField]
        private TierScaling baseDamageByTier;

        [SerializeField]
        private float marginBonus = 1f;

        public override void Resolve(in SkillEffectContext context)
        {
            var dmg = baseDamageByTier.For(context.Performance.Tier);

            if (TryGetResolution(context.Performance, out var resolution))
            {
                dmg *= 1f + marginBonus * resolution.Outcome.Margin;
            }

            var damage = Mathf.Max(1, Mathf.RoundToInt(dmg));

            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
