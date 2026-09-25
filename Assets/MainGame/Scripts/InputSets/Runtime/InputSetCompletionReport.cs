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
    }
}
