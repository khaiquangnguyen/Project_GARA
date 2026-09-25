using UnityEngine;

namespace GARA.Characters.Gambler
{
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Games/Dice Game")]
    public class DiceGameDefinition : GamblingGameDefinition
    {
        public enum GoalMode
        {
            AtLeast,
            Exactly
        }

        [SerializeField, Min(2)]
        private int dieSize = 6;

        [SerializeField]
        private GoalMode goalMode = GoalMode.AtLeast;

        [SerializeField]
        private int goalValue = 5;

        [SerializeField, Min(0)]
        private float extraDiceScale = 4f;

        public override GamblingGameType GameType => GamblingGameType.Dice;

        public override float PredictSuccessChance(float manipulate)
        {
            var extraDice = Mathf.FloorToInt(manipulate * extraDiceScale);
            var totalDice = 1 + extraDice;
            var perDieHit = goalMode == GoalMode.AtLeast
                ? Mathf.Clamp01((dieSize - goalValue + 1) / (float)dieSize)
                : Mathf.Clamp01(1f / dieSize);

            return 1f - Mathf.Pow(1f - perDieHit, totalDice);
        }

        public override GambleOutcome Resolve(GambleContext context, IGambleRandom rng)
        {
            var extraDice = Mathf.FloorToInt(context.Manipulate * extraDiceScale);
            var fractional = context.Manipulate * extraDiceScale - extraDice;
            var totalDice = 1 + extraDice + (rng.Value01() < fractional ? 1 : 0);

            var rolls = new int[totalDice];
            var best = 0;
            for (var i = 0; i < totalDice; i++)
            {
                rolls[i] = rng.Range(1, dieSize + 1);
                if (rolls[i] > best)
                {
                    best = rolls[i];
                }
            }

            var hit = goalMode == GoalMode.AtLeast ? best >= goalValue : best == goalValue;
            var jackpot = rolls[0] == dieSize;
            var result = jackpot ? GambleResult.Jackpot : (hit ? GambleResult.Success : GambleResult.Fail);
            var margin = Mathf.Clamp01(best / (float)dieSize);

            return new GambleOutcome(result, margin, PredictSuccessChance(context.Manipulate), 0f, new DiceRollDetails(rolls, best, goalValue));
        }
    }
}
