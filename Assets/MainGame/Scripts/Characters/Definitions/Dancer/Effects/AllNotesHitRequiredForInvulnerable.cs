using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Makes the user take no damage for a while, only when every note of
    // the step is hit.
    [Serializable]
    public class AllNotesHitRequiredForInvulnerable : RhythmStepEffect
    {
        [Tooltip("Turns of the user's own; 1 lasts until its next turn starts (statuses tick at turn start).")]
        [Min(1)]
        public int turns = 1;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.TotalNotes > 0 && report.MissCount == 0;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            context.Self?.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Invulnerable, turns, "invulnerable") { negatesHits = true });
        }
    }
}
