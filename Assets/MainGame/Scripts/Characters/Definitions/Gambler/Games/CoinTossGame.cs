using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    [Serializable]
    public class CoinTossGame : GamblingGame<CoinTossOutcome>
    {
        [Range(0, 1)]
        public float baseHeadsChance = 0.5f;

        [Tooltip("Heads chance added at full luck.")]
        [Range(0, 1)]
        public float luckBias = 0.3f;

        protected override CoinTossOutcome RollTyped(float luck, IGambleRandom rng)
        {
            var headsChance = Mathf.Clamp01(baseHeadsChance + luckBias * luck);
            return new CoinTossOutcome(rng.Value01() < headsChance);
        }
    }
}
