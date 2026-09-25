namespace GARA.Characters.Gambler
{
    public readonly struct PlayingCard
    {
        public readonly int Rank;
        public readonly int Suit;

        public PlayingCard(int rank, int suit)
        {
            Rank = rank;
            Suit = suit;
        }
    }
}
