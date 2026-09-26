namespace GARA.Characters.Gambler
{
    // Rank 2-14 (ace high), suit 0-3.
    public readonly struct PlayingCard
    {
        private const string RankNames = "23456789TJQKA";
        private const string SuitNames = "SHDC";

        public readonly int Rank;
        public readonly int Suit;

        public PlayingCard(int rank, int suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public override string ToString()
        {
            return $"{RankNames[Rank - 2]}{SuitNames[Suit]}";
        }
    }
}
