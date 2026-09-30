using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Hardens the Mermaid herself — less damage taken — when its gate passes.
    [Serializable]
    public class HardenSelf : GatedRhythmStepEffect
    {
        [Range(0f, 1f)]
        public float damageTakenMultiplier = 0.7f;

        [Tooltip("Stacks (turns of her own); reapplying adds stacks, one is lost per turn.")]
        [Min(1)]
        public int stacks = 2;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            context.Self?.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Hardened, stacks, "harden") { incomingDamageMultiplier = damageTakenMultiplier });
        }
    }
}
