using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Powers up every target — its skills deal more damage through its next
    // turn — only when every note of the step is hit.
    [Serializable]
    public class AllNotesHitRequiredForPower : RhythmStepEffect
    {
        [Min(0f)]
        public float damageIncrease = 0.5f;

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
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Powered, turns, "power") { outgoingDamageMultiplier = 1f + damageIncrease });
            }
        }
    }
}
