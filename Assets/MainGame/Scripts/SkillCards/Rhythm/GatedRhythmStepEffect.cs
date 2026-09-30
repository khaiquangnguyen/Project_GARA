using System;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // A step effect whose gate is picked per card instead of fixed in code.
    [Serializable]
    public abstract class GatedRhythmStepEffect : RhythmStepEffect
    {
        [Tooltip("What the step's notes must achieve for this effect to apply.")]
        public StepGate gate = StepGate.Always;

        public override bool IsTriggered(RhythmCompletionReport report) => gate.Passes(report);
    }
}
