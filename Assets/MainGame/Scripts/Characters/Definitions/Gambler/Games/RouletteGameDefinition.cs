using UnityEngine;

namespace GARA.Characters.Gambler
{
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Games/Roulette Game")]
    public class RouletteGameDefinition : GamblingGameDefinition
    {
        [SerializeField, Min(1)]
        private int chambers = 6;

        [SerializeField, Min(0)]
        private int loaded = 1;

        [SerializeField, Range(0, 1)]
        private float spinControl = 0.5f;

        [SerializeField, Range(0, 1)]
        private float backfireFractionOfMaxHp = 0.3f;

        public override GamblingGameType GameType => GamblingGameType.RussianRoulette;

        public override float PredictSuccessChance(float manipulate)
        {
            var fireChance = (loaded / (float)chambers) * Mathf.Clamp01(1f - spinControl * manipulate);
            return 1f - fireChance;
        }

        public override GambleOutcome Resolve(GambleContext context, IGambleRandom rng)
        {
            var fireChance = Mathf.Clamp01((loaded / (float)chambers) * (1f - spinControl * context.Manipulate));
            var fired = rng.Value01() < fireChance;
            var result = fired ? GambleResult.Bust : GambleResult.Jackpot;
            var backfire = fired ? backfireFractionOfMaxHp : 0f;

            return new GambleOutcome(result, fired ? 0f : 1f, 1f - fireChance, backfire, new RouletteDetails(chambers, loaded, fired));
        }
    }
}
