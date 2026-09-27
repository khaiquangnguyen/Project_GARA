using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Filmmaker
{
    // Records every target (see RecordingPassive), only when the step's
    // code is cleared.
    [Serializable]
    public class ClearedForRecording : InputSetStepEffect
    {
        [Min(1)]
        public int stacks = 1;

        public override bool IsTriggered(InputSetCompletionReport report)
        {
            return report.TotalSets > 0 && report.ClearedSets == report.TotalSets;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                RecordingPassive.Record(context.Self, target, stacks);
            }
        }
    }
}
