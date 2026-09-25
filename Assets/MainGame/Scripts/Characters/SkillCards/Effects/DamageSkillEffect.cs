using UnityEngine;

namespace GARA.Characters
{
    // Flat-damage effect scaled by performance tier and by attacker/defender
    // stats. Applies via ICombatTarget.ApplyDamage — the same standalone
    // method DamagingSpineAnimationState calls off its Spine "hit" event —
    // so this bypasses that animation-event timing entirely; whatever wires
    // up SkillCardDefinition.effects (Combat side, out of scope here) is
    // expected to call Resolve at the moment it wants the hit to land, e.g.
    // from CharacterStateContext.OnImpact.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Damage", fileName = "DamageSkillEffect")]
    public class DamageSkillEffect : SkillEffectDefinition
    {
        [CanBeRange]
        public RangeableInt baseDamageAmount;

        public TierScaling tierMultiplier;

        public StatScaling attackerScaling;

        public StatScaling defenderScaling;

        public bool scaleContinuouslyByScore;

        public override void Resolve(in SkillEffectContext context)
        {
            var attackerStats = context.Self?.CurrentStats;
            var attackerOffensiveStatValue = attackerStats != null ? attackerStats.Attack.Value : 0;

            var tierMult = scaleContinuouslyByScore
                ? Mathf.Lerp(tierMultiplier.miss, tierMultiplier.perfect, context.Performance.Score)
                : tierMultiplier.For(context.Performance.Tier);

            var attackFactor = attackerScaling.Evaluate(attackerOffensiveStatValue);
            var baseDamage = baseDamageAmount.Resolve(in context) * context.Share;

            foreach (var target in context.Targets)
            {
                var defenderOffensiveStatValue = target.CurrentStats.Defense.Value;
                var defenseFactor = defenderScaling.Evaluate(defenderOffensiveStatValue);

                var damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * tierMult * attackFactor - defenseFactor));
                target.ApplyDamage(damage);
            }
        }
    }
}
