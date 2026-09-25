using GARA.Input;

namespace GARA.InputCombos
{
    /// <summary>
    /// Raised when an in-progress combo attempt resets. Received is null unless Reason is WrongInput.
    /// </summary>
    public readonly struct ComboReset
    {
        public ComboResetReason Reason { get; }
        public int StepsReached { get; }
        public InputToken? Received { get; }
        public float TimeSinceComboStart { get; }

        public ComboReset(ComboResetReason reason, int stepsReached, InputToken? received, float timeSinceComboStart)
        {
            Reason = reason;
            StepsReached = stepsReached;
            Received = received;
            TimeSinceComboStart = timeSinceComboStart;
        }
    }
}
