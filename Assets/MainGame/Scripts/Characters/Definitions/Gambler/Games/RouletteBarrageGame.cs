using System;
using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Pulls the trigger shots times. Spun: each pull is its own
    // loaded/chambers roll. Unspun: pulls work through the cylinder, so a
    // full cylinder's worth fires every loaded bullet, then it reloads.
    [Serializable]
    public class RouletteBarrageGame : GamblingGame<RouletteBarrageOutcome>, IPerPressGame
    {
        [Tooltip("Pulls per roll. Played per press, the most presses allowed; 0 = no cap.")]
        [Min(0)]
        public int shots = 6;

        [Min(1)]
        public int chambers = 6;

        [Min(0)]
        public int loaded = 1;

        [Tooltip("Cheat: bullets loaded on top of loaded.")]
        [Min(0)]
        public int cheatBullets = 1;

        [Tooltip("Spin the cylinder before every pull. Off: a chamber once pulled is spent, so chambers pulls always fire every loaded bullet.")]
        public bool spinEachPull = true;

        public int MaxPresses => shots;

        public bool RollPress(IReadOnlyList<bool> landed, IGambleRandom rng)
        {
            return RollShot(landed, false, rng);
        }

        public GambleOutcome OutcomeOf(IReadOnlyList<bool> landed)
        {
            return new RouletteBarrageOutcome(landed, chambers, LoadedCount(false));
        }

        public int LoadedCount(bool cheated)
        {
            return Mathf.Min(loaded + (cheated ? cheatBullets : 0), chambers);
        }

        // The next pull after fired: true if it fires.
        public bool RollShot(IReadOnlyList<bool> fired, bool cheated, IGambleRandom rng)
        {
            var loadedCount = LoadedCount(cheated);
            if (spinEachPull)
            {
                return rng.Value01() < loadedCount / (float)chambers;
            }

            // Only the current cylinder's pulls count; it reloads when spent.
            var cylinderStart = fired.Count - fired.Count % chambers;
            var spentBullets = 0;
            for (var i = cylinderStart; i < fired.Count; i++)
            {
                if (fired[i])
                {
                    spentBullets++;
                }
            }

            var chambersLeft = chambers - (fired.Count - cylinderStart);
            return rng.Value01() < (loadedCount - spentBullets) / (float)chambersLeft;
        }

        protected override RouletteBarrageOutcome RollTyped(bool cheated, IGambleRandom rng)
        {
            var fired = new List<bool>(shots);
            for (var i = 0; i < shots; i++)
            {
                fired.Add(RollShot(fired, cheated, rng));
            }

            return new RouletteBarrageOutcome(fired, chambers, LoadedCount(cheated));
        }
    }
}
