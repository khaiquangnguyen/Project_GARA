using System.Collections.Generic;

namespace GARA.InputSets
{
    /// <summary>
    /// Summary of a finished (or aborted) <see cref="InputSetCollectionRunner"/> run.
    /// </summary>
    public sealed class InputSetCompletionReport
    {
        public int TotalSets;
        public int ClearedSets;
        public int FirstTryClears;
        public int TotalAttempts;
        public float CompletionRate;
        public float FirstTryRate;
        public float AttemptEfficiency;
        public float TotalElapsed;
        public bool WasAborted;
        public bool SetCollectionTimedOut;
        public IReadOnlyList<InputSetResult> SetResults;

        // Scores one finished set as if it were a whole run.
        public static InputSetCompletionReport ForSingleSet(InputSetResult result)
        {
            var cleared = result.Outcome == InputSetOutcome.Cleared;
            var firstTry = cleared && result.ClearedFirstTry;
            return new InputSetCompletionReport
            {
                TotalSets = 1,
                ClearedSets = cleared ? 1 : 0,
                FirstTryClears = firstTry ? 1 : 0,
                TotalAttempts = result.Attempts,
                CompletionRate = cleared ? 1f : 0f,
                FirstTryRate = firstTry ? 1f : 0f,
                AttemptEfficiency = result.Attempts > 0 ? 1f / result.Attempts : 0f,
                TotalElapsed = result.TimeInSet,
                WasAborted = false,
                SetCollectionTimedOut = false,
                SetResults = new[] { result }
            };
        }
    }
}
