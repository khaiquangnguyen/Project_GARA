namespace GARA.Characters.Gambler
{
    public readonly struct CardDrawDetails
    {
        public readonly PlayingCard Drawn;
        public readonly PlayingCard House;
        public readonly int CulledCount;

        public CardDrawDetails(PlayingCard drawn, PlayingCard house, int culledCount)
        {
            Drawn = drawn;
            House = house;
            CulledCount = culledCount;
        }
    }
}
