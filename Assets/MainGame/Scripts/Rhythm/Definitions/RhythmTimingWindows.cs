using System;

namespace GARA.Rhythm
{
    /// <summary>Half-width timing tolerances used to grade how close a press landed to a note.</summary>
    [Serializable]
    public struct RhythmTimingWindows
    {
        public float perfect;
        public float good;
        public float ok;
        public float holdReleaseTolerance;

        /// <summary>
        /// Grades a press by its absolute distance from the note's target time. Outside
        /// <see cref="ok"/> there is no match at all (callers should treat that as a miss).
        /// </summary>
        public RhythmJudgement Judge(float absDelta)
        {
            if (absDelta > ok)
            {
                return RhythmJudgement.Miss;
            }

            if (absDelta > good)
            {
                return RhythmJudgement.Ok;
            }

            if (absDelta > perfect)
            {
                return RhythmJudgement.Good;
            }

            return RhythmJudgement.Perfect;
        }
    }
}
