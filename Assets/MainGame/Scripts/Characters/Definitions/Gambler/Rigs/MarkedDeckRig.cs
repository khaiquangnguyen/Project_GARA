using UnityEngine;

namespace GARA.Characters.Gambler
{
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Rigs/Marked Deck Rig")]
    public class MarkedDeckRig : GambleRigDefinition
    {
        [SerializeField]
        private float cullMultiplier = 2f;

        public override void BeforeResolve(GambleContext context)
        {
            context.Manipulate *= cullMultiplier;
        }

        public override void AfterResolve(GambleContext context, ref GambleOutcome outcome)
        {
            if (outcome.GameDetails is CardDrawDetails details
                && details.Drawn.Suit == details.House.Suit
                && outcome.Result == GambleResult.Success)
            {
                outcome = new GambleOutcome(GambleResult.Jackpot, outcome.Margin, outcome.FinalSuccessChance, 0f, outcome.GameDetails);
            }
        }
    }
}
