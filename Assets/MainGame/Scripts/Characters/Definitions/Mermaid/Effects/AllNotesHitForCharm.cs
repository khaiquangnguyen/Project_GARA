using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Charms every target (see SuperstarSingerPassive), only when every note
    // of the step is hit.
    [Serializable]
    public class AllNotesHitForCharm : RhythmStepEffect
    {
        [Min(1)]
        public int charm = 25;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.TotalNotes > 0 && report.MissCount == 0;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                SuperstarSingerPassive.AddCharm(context.Self, target, charm);
            }
        }
    }
}
