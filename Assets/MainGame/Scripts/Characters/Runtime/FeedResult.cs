namespace GARA.Characters
{
    // Outcome of one CombatParticipant.Feed call.
    public readonly struct FeedResult
    {
        public readonly int amountGained;
        public readonly int fullness;
        public readonly int capacity;
        public readonly bool becameFull;
        public readonly int overflow;

        public FeedResult(int amountGained, int fullness, int capacity, bool becameFull, int overflow)
        {
            this.amountGained = amountGained;
            this.fullness = fullness;
            this.capacity = capacity;
            this.becameFull = becameFull;
            this.overflow = overflow;
        }

        public static FeedResult NotFeedable => new FeedResult(0, 0, 0, false, 0);
    }
}
