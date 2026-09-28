namespace GARA.ShakeBalance
{
    /// <summary>
    /// Summary of a finished (or aborted) <see cref="ShakeBalanceRunner"/> run. A class so it can
    /// ride along as a SkillPerformance's Details payload.
    /// </summary>
    public sealed class ShakeBalanceReport
    {
        /// <summary>1 when the meter filled in time, else 0.</summary>
        public float Result => Succeeded ? 1f : 0f;

        /// <summary>Meter at the end of the run, 0-1.</summary>
        public float Meter;

        public float Elapsed;

        public float PerfectTime;
        public float GoodTime;
        public float OffTime;
        public int Pushes;

        /// <summary>Instability reached at the end of the run — how far into the difficulty ramp the player got.</summary>
        public float FinalInstability;

        public ShakeBalanceEndReason EndReason;

        public bool Succeeded => EndReason == ShakeBalanceEndReason.Succeeded;
        public bool Fell => EndReason == ShakeBalanceEndReason.Fell;
        public bool WasAborted => EndReason == ShakeBalanceEndReason.Aborted;
    }
}
