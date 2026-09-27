using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Heals every target (or just the user), only when every note of the
    // step is hit.
    [Serializable]
    public class AllNotesHitRequiredForHeal : RhythmStepEffect
    {
        [Min(1)]
        public int heal = 10;

        [Tooltip("Heal only the user, not the card's targets.")]
        public bool selfOnly;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.TotalNotes > 0 && report.MissCount == 0;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            if (selfOnly)
            {
                context.Self?.Heal(heal);
                return;
            }

            foreach (var target in context.Targets)
            {
                target.Heal(heal);
            }
        }
    }
}
