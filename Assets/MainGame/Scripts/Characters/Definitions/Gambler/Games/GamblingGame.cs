using System;

namespace GARA.Characters.Gambler
{
    // Rolled when a Gambler card is played; luck (0-1) tilts the odds.
    [Serializable]
    public abstract class GamblingGame
    {
        public abstract Type OutcomeType { get; }

        public abstract GambleOutcome Roll(float luck, IGambleRandom rng);
    }

    [Serializable]
    public abstract class GamblingGame<TOutcome> : GamblingGame where TOutcome : GambleOutcome
    {
        public override Type OutcomeType => typeof(TOutcome);

        public override GambleOutcome Roll(float luck, IGambleRandom rng)
        {
            return RollTyped(luck, rng);
        }

        protected abstract TOutcome RollTyped(float luck, IGambleRandom rng);
    }
}
