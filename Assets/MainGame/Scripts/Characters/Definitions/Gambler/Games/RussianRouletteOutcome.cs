namespace GARA.Characters.Gambler
{
    public sealed class RussianRouletteOutcome : GambleOutcome
    {
        public readonly bool Fired;
        public readonly int Chamber;
        public readonly int Chambers;
        public readonly int Loaded;

        public RussianRouletteOutcome(bool fired, int chamber, int chambers, int loaded)
        {
            Fired = fired;
            Chamber = chamber;
            Chambers = chambers;
            Loaded = loaded;
        }

        public override float Significance => Fired ? 0f : 1f;

        public override string ToString()
        {
            return $"Roulette: {(Fired ? "BANG" : "click")} ({Loaded}/{Chambers} loaded)";
        }
    }
}
