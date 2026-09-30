using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Hardens every ally among the targets — less damage taken — when its
    // gate passes.
    [Serializable]
    public class ApplyHarden : GatedRhythmStepEffect
    {
        [Range(0f, 1f)]
        public float damageTakenMultiplier = 0.75f;

        [Tooltip("Stacks (turns of the target's own); reapplying adds stacks, one is lost per turn.")]
        [Min(1)]
        public int stacks = 1;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                if (target.Faction == context.Self.Faction)
                {
                    target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Hardened, stacks, "harden") { incomingDamageMultiplier = damageTakenMultiplier });
                }
            }
        }
    }
}
