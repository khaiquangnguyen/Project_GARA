using UnityEngine;

namespace GARA.Characters.Gambler
{
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Games/Card Game")]
    public class CardGameDefinition : GamblingGameDefinition
    {
        [SerializeField, Range(2, 14)]
        private int houseMinRank = 6;

        [SerializeField, Range(2, 14)]
        private int houseMaxRank = 11;

        [SerializeField, Range(0, 1)]
        private float cullFraction = 0.5f;

        public override GamblingGameType GameType => GamblingGameType.Card;

        public override float PredictSuccessChance(float manipulate)
        {
            var avgHouse = (houseMinRank + houseMaxRank) / 2f;
            var culled = Mathf.Clamp01(manipulate * cullFraction);
            var effectiveMin = Mathf.Lerp(2f, avgHouse, culled);

            return Mathf.Clamp01((14f - avgHouse) / (14f - effectiveMin + 1f));
        }

        public override GambleOutcome Resolve(GambleContext context, IGambleRandom rng)
        {
            var culledCount = Mathf.RoundToInt(Mathf.Clamp01(context.Manipulate * cullFraction) * 52);
            var minRank = Mathf.Clamp(2 + culledCount / 4, 2, 14);
            var playerRank = rng.Range(minRank, 15);
            var playerSuit = rng.Range(0, 4);
            var houseRank = rng.Range(houseMinRank, houseMaxRank + 1);
            var houseSuit = rng.Range(0, 4);

            var drawn = new PlayingCard(playerRank, playerSuit);
            var house = new PlayingCard(houseRank, houseSuit);

            var ace = playerRank == 14;
            var tie = playerRank == houseRank;

            GambleResult result;
            if (ace)
            {
                result = GambleResult.Jackpot;
            }
            else if (tie)
            {
                result = GambleResult.Fail;
            }
            else if (playerRank > houseRank)
            {
                result = GambleResult.Success;
            }
            else
            {
                result = GambleResult.Fail;
            }

            var margin = Mathf.Clamp01((playerRank - houseRank + 12) / 24f);

            return new GambleOutcome(result, margin, PredictSuccessChance(context.Manipulate), 0f, new CardDrawDetails(drawn, house, culledCount));
        }
    }
}
