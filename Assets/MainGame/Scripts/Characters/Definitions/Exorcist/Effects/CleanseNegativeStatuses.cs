using System;
using GARA.SkillCards.Rhythm;

namespace GARA.Characters.Exorcist
{
    // Removes every negative status from every target, when its gate passes.
    [Serializable]
    public class CleanseNegativeStatuses : GatedRhythmStepEffect
    {
        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.TakeStatuses(status => status.kind.IsNegative());
            }
        }
    }
}
