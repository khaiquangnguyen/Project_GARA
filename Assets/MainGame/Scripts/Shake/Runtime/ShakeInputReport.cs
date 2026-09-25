namespace GARA.Shake
{
    /// <summary>A snapshot of a shake-input run, taken mid-run or at completion.</summary>
    public readonly struct ShakeInputReport
    {
        public int CompletedPairs { get; }
        public int AcceptedPresses { get; }
        public int WrongPresses { get; }
        public int BestStreak { get; }
        public float Elapsed { get; }
        public float PairsPerSecond { get; }
        public bool WasStoppedExternally { get; }

        public ShakeInputReport(int completedPairs, int acceptedPresses, int wrongPresses, int bestStreak, float elapsed, bool wasStoppedExternally)
        {
            CompletedPairs = completedPairs;
            AcceptedPresses = acceptedPresses;
            WrongPresses = wrongPresses;
            BestStreak = bestStreak;
            Elapsed = elapsed;
            WasStoppedExternally = wasStoppedExternally;
            PairsPerSecond = elapsed > 0f ? completedPairs / elapsed : 0f;
        }
    }
}
