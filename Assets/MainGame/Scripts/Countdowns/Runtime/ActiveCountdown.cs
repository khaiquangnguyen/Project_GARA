namespace GARA.Countdowns
{
    /// <summary>
    /// A read-only snapshot of one currently-active countdown, for UI consumption.
    /// Progress is 0 at spawn and 1 at expiry.
    /// </summary>
    public readonly struct ActiveCountdown
    {
        public int Id { get; }
        public float Remaining { get; }
        public float Duration { get; }
        public float Progress { get; }
        public bool IsInPerfectWindow { get; }
        public float PerfectWindow { get; }
        public float PerfectOffsetFromEnd { get; }

        public ActiveCountdown(int id, float remaining, float duration, bool isInPerfectWindow, float perfectWindow, float perfectOffsetFromEnd)
        {
            Id = id;
            Remaining = remaining;
            Duration = duration;
            Progress = duration > 0f ? 1f - (remaining / duration) : 1f;
            IsInPerfectWindow = isInPerfectWindow;
            PerfectWindow = perfectWindow;
            PerfectOffsetFromEnd = perfectOffsetFromEnd;
        }
    }
}
