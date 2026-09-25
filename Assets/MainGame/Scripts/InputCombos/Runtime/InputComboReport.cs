namespace GARA.InputCombos
{
    /// <summary>
    /// Final summary of a completed or stopped input combo session.
    /// </summary>
    public sealed class InputComboReport
    {
        public float TotalScore;
        public int CompletedCombos;
        public int AttemptsStarted;
        public int Resets;
        public int ResetsWrongInput;
        public int ResetsTimedOut;
        public int StepsAccepted;
        public int BestStepsReached;
        public int IdlePresses;
        public float Elapsed;
        public bool WasStopped;
        public float CompletionRate;
    }
}
