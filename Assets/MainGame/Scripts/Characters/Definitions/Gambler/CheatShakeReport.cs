namespace GARA.Characters.Gambler
{
    public sealed class CheatShakeReport
    {
        public int Required { get; }

        public int Count { get; }

        public float Elapsed { get; }

        public CheatShakeEndReason EndReason { get; }

        public bool WasAborted => EndReason == CheatShakeEndReason.Aborted;

        // Stopped on the exact count, by pressing done or by running out of time.
        public bool Cleared => (EndReason == CheatShakeEndReason.Done || EndReason == CheatShakeEndReason.TimeUp) && Count == Required;

        public CheatShakeReport(int required, int count, float elapsed, CheatShakeEndReason endReason)
        {
            Required = required;
            Count = count;
            Elapsed = elapsed;
            EndReason = endReason;
        }
    }
}
