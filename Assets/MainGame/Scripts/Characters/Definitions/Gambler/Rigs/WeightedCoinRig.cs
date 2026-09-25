using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Filed-down two-header: bumps the coin's success chance and disables
    // its Jackpot edge case (a shaved coin can't land on its edge), instead
    // downgrading any Jackpot roll to a plain Success and softening a Fail
    // into a smaller backfire rather than a full Bust.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Rigs/Weighted Coin Rig")]
    public class WeightedCoinRig : GambleRigDefinition
    {
        [SerializeField, Range(0, 1)]
        private float successBonus = 0.25f;

        [SerializeField, Range(0, 1)]
        private float failBackfireFraction = 0.1f;

        public override void BeforeResolve(GambleContext context)
        {
            context.Manipulate = Mathf.Clamp01(context.Manipulate + successBonus);
        }

        public override void AfterResolve(GambleContext context, ref GambleOutcome outcome)
        {
            if (outcome.Result == GambleResult.Fail)
            {
                outcome = new GambleOutcome(GambleResult.Bust, outcome.Margin, outcome.FinalSuccessChance, failBackfireFraction, outcome.GameDetails);
            }
            else if (outcome.Result == GambleResult.Jackpot)
            {
                outcome = new GambleOutcome(GambleResult.Success, outcome.Margin, outcome.FinalSuccessChance, 0f, outcome.GameDetails);
            }
        }
    }
}
