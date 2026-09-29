using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Characters.Gambler
{
    // Tosses every coin on its own. Cheat: a better heads chance. Played per
    // press, each press tosses one more coin at headsChance, uncapped.
    [Serializable]
    public class CoinTossGame : GamblingGame<CoinTossOutcome>, IPerPressGame
    {
        [Min(1)]
        public int coins = 1;

        [FormerlySerializedAs("baseHeadsChance")]
        [Range(0, 1)]
        public float headsChance = 0.5f;

        [Range(0, 1)]
        public float cheatHeadsChance = 0.6f;

        public int MaxPresses => 0;

        public bool RollPress(IReadOnlyList<bool> landed, IGambleRandom rng)
        {
            return rng.Value01() < headsChance;
        }

        public GambleOutcome OutcomeOf(IReadOnlyList<bool> landed)
        {
            return new CoinTossOutcome(landed);
        }

        protected override CoinTossOutcome RollTyped(bool cheated, IGambleRandom rng)
        {
            var chance = cheated ? cheatHeadsChance : headsChance;
            var faces = new bool[coins];
            for (var i = 0; i < faces.Length; i++)
            {
                faces[i] = rng.Value01() < chance;
            }

            return new CoinTossOutcome(faces);
        }
    }
}
