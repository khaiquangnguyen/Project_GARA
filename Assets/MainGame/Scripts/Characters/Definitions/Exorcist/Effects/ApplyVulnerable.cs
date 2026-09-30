using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Makes every opponent among the targets Vulnerable — more damage taken —
    // when its gate passes.
    [Serializable]
    public class ApplyVulnerable : GatedRhythmStepEffect
    {
        [Min(1f)]
        public float damageTakenMultiplier = 1.25f;

        [Tooltip("Stacks (turns of the target's own); reapplying adds stacks, one is lost per turn.")]
        [Min(1)]
        public int stacks = 1;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                if (target.Faction != context.Self.Faction)
                {
                    target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Vulnerable, stacks, "vulnerable") { incomingDamageMultiplier = damageTakenMultiplier });
                }
            }
        }
    }
}
