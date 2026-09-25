namespace GARA.ShakeBalance
{
    /// <summary>
    /// Summary of a finished (or aborted) <see cref="ShakeBalanceRunner"/> run. A class so it can
    /// ride along as a SkillPerformance's Details payload.
    /// </summary>
    public sealed class ShakeBalanceReport
    {
        /// <summary>Final 0-1 result, after the fall penalty.</summary>
        public float Result;

        /// <summary>Result before the fall penalty.</summary>
        public float UnpenalizedResult;

        /// <summary>Quality-weighted seconds banked (1 per second in the perfect zone).</summary>
        public float Bank;

        public float Elapsed;

        /// <summary>Bank / Elapsed: how well-balanced the run was on average, independent of its length.</summary>
        public float AverageQuality;

        public float PerfectTime;
        public float GoodTime;
        public float OffTime;
        public int Pushes;

        /// <summary>Instability reached at the end of the run — how far into the difficulty ramp the player got.</summary>
        public float FinalInstability;

        public ShakeBalanceEndReason EndReason;

        public bool Fell => EndReason == ShakeBalanceEndReason.Fell;
        public bool WasAborted => EndReason == ShakeBalanceEndReason.Aborted;
    }
}
