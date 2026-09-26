using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    [Serializable]
    public class RussianRouletteGame : GamblingGame<RussianRouletteOutcome>
    {
        [Min(1)]
        public int chambers = 6;

        [Min(0)]
        public int loaded = 1;

        [Tooltip("Fraction of the fire chance removed at full luck.")]
        [Range(0, 1)]
        public float luckSafety = 0.5f;

        protected override RussianRouletteOutcome RollTyped(float luck, IGambleRandom rng)
        {
            var loadedCount = Mathf.Min(loaded, chambers);
            var fireChance = loadedCount / (float)chambers * (1f - luckSafety * luck);
            var fired = rng.Value01() < fireChance;
            return new RussianRouletteOutcome(fired, rng.Range(0, chambers), chambers, loadedCount);
        }
    }
}
