namespace GARA.InputSets
{
    /// <summary>
    /// The outcome of a single set within a set collection run.
    /// </summary>
    public readonly struct InputSetResult
    {
        public readonly int SetIndex;
        public readonly InputSetOutcome Outcome;
        public readonly int Attempts;
        public readonly bool ClearedFirstTry;
        public readonly float TimeToClear;
        public readonly float TimeInSet;
        public readonly InputSetFailureReason LastFailure;
        public readonly int BestProgress;

        public InputSetResult(
            int setIndex,
            InputSetOutcome outcome,
            int attempts,
            bool clearedFirstTry,
            float timeToClear,
            float timeInSet,
            InputSetFailureReason lastFailure,
            int bestProgress)
        {
            SetIndex = setIndex;
            Outcome = outcome;
            Attempts = attempts;
            ClearedFirstTry = clearedFirstTry;
            TimeToClear = timeToClear;
            TimeInSet = timeInSet;
            LastFailure = lastFailure;
            BestProgress = bestProgress;
        }
    }
}
