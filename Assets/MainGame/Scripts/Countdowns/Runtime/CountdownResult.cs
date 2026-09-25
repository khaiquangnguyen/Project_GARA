namespace GARA.Countdowns
{
    /// <summary>
    /// The final outcome of one countdown, whether resolved by a press, expired, or cancelled.
    /// RemainingAtResolve is NaN when Outcome is Missed.
    /// </summary>
    public readonly struct CountdownResult
    {
        public int Id { get; }
        public CountdownOutcome Outcome { get; }
        public float SpawnTime { get; }
        public float Duration { get; }
        public float RemainingAtResolve { get; }
        public bool WasAuthored { get; }

        public CountdownResult(int id, CountdownOutcome outcome, float spawnTime, float duration, float remainingAtResolve, bool wasAuthored)
        {
            Id = id;
            Outcome = outcome;
            SpawnTime = spawnTime;
            Duration = duration;
            RemainingAtResolve = remainingAtResolve;
            WasAuthored = wasAuthored;
        }
    }
}
