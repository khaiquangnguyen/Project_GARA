using System;

namespace GARA.Characters.Gambler
{
    // Rolled when a Gambler card is played; a successful cheat (the QTE
    // hit) tips the roll the game's own way.
    [Serializable]
    public abstract class GamblingGame
    {
        public abstract Type OutcomeType { get; }

        public abstract GambleOutcome Roll(bool cheated, IGambleRandom rng);
    }

    [Serializable]
    public abstract class GamblingGame<TOutcome> : GamblingGame where TOutcome : GambleOutcome
    {
        public override Type OutcomeType => typeof(TOutcome);

        public override GambleOutcome Roll(bool cheated, IGambleRandom rng)
        {
            return RollTyped(cheated, rng);
        }

        protected abstract TOutcome RollTyped(bool cheated, IGambleRandom rng);
    }
}
