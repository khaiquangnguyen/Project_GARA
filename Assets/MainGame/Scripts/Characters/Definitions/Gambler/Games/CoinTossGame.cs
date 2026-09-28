using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Characters.Gambler
{
    // Tosses every coin on its own. Cheat: a better heads chance.
    [Serializable]
    public class CoinTossGame : GamblingGame<CoinTossOutcome>
    {
        [Min(1)]
        public int coins = 1;

        [FormerlySerializedAs("baseHeadsChance")]
        [Range(0, 1)]
        public float headsChance = 0.5f;

        [Range(0, 1)]
        public float cheatHeadsChance = 0.6f;

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
