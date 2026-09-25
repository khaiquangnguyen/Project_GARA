using UnityEngine;

namespace GARA.Characters.Gambler
{
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Games/Coin Game")]
    public class CoinGameDefinition : GamblingGameDefinition
    {
        [SerializeField, Range(0, 1)]
        private float baseSuccessRate = 0.5f;

        [SerializeField, Range(0, 1)]
        private float edgeChance = 0.02f;

        public override GamblingGameType GameType => GamblingGameType.Coin;

        public override float PredictSuccessChance(float manipulate)
        {
            return Mathf.Clamp01(baseSuccessRate + manipulate);
        }

        public override GambleOutcome Resolve(GambleContext context, IGambleRandom rng)
        {
            var successChance = PredictSuccessChance(context.Manipulate);
            var edgeC = Mathf.Clamp01(edgeChance * (1f + context.Manipulate));
            var roll = rng.Value01();
            var edge = roll < edgeC;
            var heads = edge || roll < successChance;

            var result = edge ? GambleResult.Jackpot : (heads ? GambleResult.Success : GambleResult.Fail);
            var margin = heads ? successChance : (1f - successChance);

            return new GambleOutcome(result, margin, successChance, 0f, new CoinFlipDetails(heads, edge));
        }
    }
}
