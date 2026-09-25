using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // Bridges the class-agnostic IValueRangeRoller onto the Gambler's existing
    // IGambleRandom seam, so range rolls are deterministic under SeededGambleRandom
    // in tests, exactly like gambling-game resolution.
    public sealed class GambleRangeRoller : IValueRangeRoller
    {
        private readonly IGambleRandom _rng;

        public GambleRangeRoller(IGambleRandom rng)
        {
            _rng = rng;
        }

        public int RollRange(int minInclusive, int maxInclusive)
        {
            return _rng.Range(minInclusive, maxInclusive + 1);
        }

        public float RollRange(float min, float max)
        {
            return min + _rng.Value01() * (max - min);
        }
    }
}
