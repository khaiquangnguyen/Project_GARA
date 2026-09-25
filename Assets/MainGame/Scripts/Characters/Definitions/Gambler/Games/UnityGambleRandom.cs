namespace GARA.Characters.Gambler
{
    public sealed class UnityGambleRandom : IGambleRandom
    {
        public float Value01()
        {
            return UnityEngine.Random.value;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
