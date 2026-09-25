namespace GARA.Characters.Gambler
{
    public readonly struct CoinFlipDetails
    {
        public readonly bool Heads;
        public readonly bool Edge;

        public CoinFlipDetails(bool heads, bool edge)
        {
            Heads = heads;
            Edge = edge;
        }
    }
}
