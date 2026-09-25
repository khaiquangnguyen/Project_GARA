using System;

namespace GARA.Rhythm
{
    /// <summary>A sequence's timing around its notes: lead-in, tail-out and judging windows.</summary>
    [Serializable]
    public struct RhythmSequenceTiming
    {
        public float leadIn;
        public float tailOut;
        public RhythmTimingWindows windows;

        public static RhythmSequenceTiming Default => new RhythmSequenceTiming
        {
            leadIn = 1f,
            tailOut = 0.25f,
            windows = new RhythmTimingWindows
            {
                perfect = 0.05f,
                good = 0.1f,
                ok = 0.15f,
                holdReleaseTolerance = 0.1f
            }
        };
    }
}
