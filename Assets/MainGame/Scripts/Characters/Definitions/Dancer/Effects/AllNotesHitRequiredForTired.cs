using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Tires every target — slowed and weakened through its next turn — only
    // when every note of the step is hit.
    [Serializable]
    public class AllNotesHitRequiredForTired : RhythmStepEffect
    {
        [Range(0f, 1f)]
        public float speedReduction = 0.25f;

        [Range(0f, 1f)]
        public float damageReduction = 0.25f;

        [Tooltip("Turns of the target's own; 2 lasts through its next turn (statuses tick at turn start).")]
        [Min(1)]
        public int turns = 2;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.TotalNotes > 0 && report.MissCount == 0;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Slowed, turns, "tired") { speedMultiplier = 1f - speedReduction });
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Weakened, turns, "tired") { outgoingDamageMultiplier = 1f - damageReduction });
            }
        }
    }
}
