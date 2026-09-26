using System;
using GARA.Characters;
using GARA.Rhythm;

namespace GARA.SkillCards.Rhythm
{
    // An effect on a rhythm bar or finale. Its gate decides from the step's
    // notes whether it applies; a step plays (move and hit) only when at
    // least one of its effects' gates passes.
    [Serializable]
    public abstract class RhythmStepEffect : ISkillEffect
    {
        // Report covers the bar's notes, or the whole run for the finale.
        public abstract bool IsTriggered(RhythmCompletionReport report);

        public abstract void ApplyEffect(in SkillEffectContext context);
    }
}
