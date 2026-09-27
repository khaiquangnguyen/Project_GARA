using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Gives the user evasion stacks — each negates one incoming hit — only
    // when every note of the step is hit. Stacks last until used.
    [Serializable]
    public class AllNotesHitRequiredForEvasion : RhythmStepEffect
    {
        [Min(1)]
        public int stacks = 1;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.TotalNotes > 0 && report.MissCount == 0;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            for (var i = 0; i < stacks; i++)
            {
                var evasion = StatusEffectInstance.Permanent(StatusEffectKind.Evasive, "evasion");
                evasion.evadesNextHit = true;
                context.Self?.ApplyStatus(evasion);
            }
        }
    }
}
