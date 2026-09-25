namespace GARA.Characters.Gambler
{
    // Randomness seam every gambling game rolls through, so resolution can be
    // driven deterministically (SeededGambleRandom) in tests/replays and by
    // UnityGambleRandom at runtime.
    public interface IGambleRandom
    {
        float Value01();

        int Range(int minInclusive, int maxExclusive);
    }
}
