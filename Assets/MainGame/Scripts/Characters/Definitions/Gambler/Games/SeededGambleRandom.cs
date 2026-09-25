using System;

namespace GARA.Characters.Gambler
{
    public sealed class SeededGambleRandom : IGambleRandom
    {
        private readonly Random _random;

        public SeededGambleRandom(int seed)
        {
            _random = new Random(seed);
        }

        public float Value01()
        {
            return (float)_random.NextDouble();
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}
