using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Weakens every target — less damage dealt — when its gate passes.
    [Serializable]
    public class ApplyWeaken : GatedRhythmStepEffect
    {
        [Range(0f, 1f)]
        public float damageReduction = 0.3f;

        [Tooltip("Turns of the target's own; 2 lasts through its next turn (statuses tick at turn start).")]
        [Min(1)]
        public int turns = 2;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Weakened, turns, "weaken") { outgoingDamageMultiplier = 1f - damageReduction });
            }
        }
    }
}
