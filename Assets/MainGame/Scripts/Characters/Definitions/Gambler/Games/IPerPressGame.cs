using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    // A game played one press at a time: each press rolls one pull or toss
    // that lands or not.
    public interface IPerPressGame
    {
        // Presses the run allows; 0 = as many as fit in the time limit.
        int MaxPresses { get; }

        // Rolls the next press, given the presses before it (true = landed).
        bool RollPress(IReadOnlyList<bool> landed, IGambleRandom rng);

        // The outcome of the presses so far, in order (true = landed).
        GambleOutcome OutcomeOf(IReadOnlyList<bool> landed);
    }
}
