namespace GARA.Characters.Gambler
{
    public readonly struct RouletteDetails
    {
        public readonly int Chambers;
        public readonly int Loaded;
        public readonly bool Fired;

        public RouletteDetails(int chambers, int loaded, bool fired)
        {
            Chambers = chambers;
            Loaded = loaded;
            Fired = fired;
        }
    }
}
