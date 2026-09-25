using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Removes the danger (no backfire) and downgrades the payoff (Jackpot
    // becomes a plain Success) — a Russian Roulette rig with the drama taken
    // out of it in both directions.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Rigs/Blank Rounds Rig")]
    public class BlankRoundsRig : GambleRigDefinition
    {
        public override void AfterResolve(GambleContext context, ref GambleOutcome outcome)
        {
            if (outcome.Result == GambleResult.Jackpot)
            {
                outcome = new GambleOutcome(GambleResult.Success, outcome.Margin, 1f, 0f, outcome.GameDetails);
            }
        }
    }
}
