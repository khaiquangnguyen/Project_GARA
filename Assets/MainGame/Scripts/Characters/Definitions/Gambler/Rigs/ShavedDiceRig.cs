using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Simplified: the design calls for shaved dice to reroll any 1s, but
    // that needs RNG access mid-resolve, and GambleRigDefinition.AfterResolve
    // only sees the already-rolled outcome (no IGambleRandom parameter).
    // Implemented here as just the "no Jackpot" downgrade; the reroll
    // behaviour needs either a richer AfterResolve(context, ref outcome, rng)
    // hook or to live inside DiceGameDefinition itself, reading a rig
    // reference/knob off the context. Flagging rather than silently dropping
    // it per the task brief.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Rigs/Shaved Dice Rig")]
    public class ShavedDiceRig : GambleRigDefinition
    {
        [SerializeField]
        private int rerollBelow = 2;

        public override void AfterResolve(GambleContext context, ref GambleOutcome outcome)
        {
            if (outcome.Result == GambleResult.Jackpot)
            {
                outcome = new GambleOutcome(GambleResult.Success, outcome.Margin, outcome.FinalSuccessChance, 0f, outcome.GameDetails);
            }
        }
    }
}
