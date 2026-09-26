namespace GARA.Characters.Gambler
{
    // One roll of a GamblingGame. Significance: how good it went, 0-1.
    public abstract class GambleOutcome
    {
        public abstract float Significance { get; }
    }
}
