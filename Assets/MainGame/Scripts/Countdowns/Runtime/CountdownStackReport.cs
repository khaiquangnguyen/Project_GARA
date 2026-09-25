using System.Collections.Generic;

namespace GARA.Countdowns
{
    /// <summary>
    /// Final summary of a completed or stopped countdown stack run.
    /// </summary>
    public sealed class CountdownStackReport
    {
        public int TotalCountdowns;
        public int SuccessCount;
        public int PerfectCount;
        public int NormalCount;
        public int MissCount;
        public int CancelledCount;
        public int WhiffPresses;
        public float SuccessRate;
        public float PerfectRate;
        public float Elapsed;
        public bool WasStopped;
        public IReadOnlyList<CountdownResult> Results;
    }
}
