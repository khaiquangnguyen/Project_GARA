using System.Collections.Generic;
using GARA.Input;
using GARA.InputSets;

namespace GARA.Chords
{
    /// <summary>
    /// A single failed chord attempt: either the required tokens spread too far apart in
    /// time, or an extra, non-required token was pressed while the chord was live.
    /// </summary>
    public readonly struct ChordFailure
    {
        public readonly int SetIndex;
        public readonly int Attempt;
        public readonly InputSetFailureReason Reason;
        public readonly float Spread;
        public readonly IReadOnlyList<InputToken> HeldAtFailure;

        public ChordFailure(int setIndex, int attempt, InputSetFailureReason reason, float spread, IReadOnlyList<InputToken> heldAtFailure)
        {
            SetIndex = setIndex;
            Attempt = attempt;
            Reason = reason;
            Spread = spread;
            HeldAtFailure = heldAtFailure;
        }
    }
}
