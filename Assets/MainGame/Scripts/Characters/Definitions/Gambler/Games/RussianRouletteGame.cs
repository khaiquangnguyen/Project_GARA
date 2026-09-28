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

        [Tooltip("Cheat: bullets loaded on top of loaded.")]
        [Min(0)]
        public int cheatBullets = 1;

        protected override RussianRouletteOutcome RollTyped(bool cheated, IGambleRandom rng)
        {
            var loadedCount = Mathf.Min(loaded + (cheated ? cheatBullets : 0), chambers);
            var fireChance = loadedCount / (float)chambers;
            var fired = rng.Value01() < fireChance;
            return new RussianRouletteOutcome(fired, rng.Range(0, chambers), chambers, loadedCount);
        }
    }
}
