using GARA.Input;

namespace GARA.InputSets
{
    /// <summary>
    /// A single mismatched or timed-out press within an in-progress set.
    /// </summary>
    public readonly struct InputSetFailure
    {
        public readonly int SetIndex;
        public readonly int Attempt;
        public readonly int InputIndex;
        public readonly InputToken Expected;
        public readonly InputToken Received;
        public readonly InputSetFailureReason Reason;

        public InputSetFailure(
            int setIndex,
            int attempt,
            int inputIndex,
            InputToken expected,
            InputToken received,
            InputSetFailureReason reason)
        {
            SetIndex = setIndex;
            Attempt = attempt;
            InputIndex = inputIndex;
            Expected = expected;
            Received = received;
            Reason = reason;
        }
    }
}
