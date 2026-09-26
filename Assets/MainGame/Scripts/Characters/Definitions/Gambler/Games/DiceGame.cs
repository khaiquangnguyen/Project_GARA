using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Rolls count dice; luck rerolls the lowest die, keeping the better face.
    [Serializable]
    public class DiceGame : GamblingGame<DiceOutcome>
    {
        [Min(1)]
        public int count = 2;

        [Min(2)]
        public int sides = 6;

        [Tooltip("Rerolls of the lowest die at full luck.")]
        [Min(0)]
        public int maxLuckRerolls = 2;

        protected override DiceOutcome RollTyped(float luck, IGambleRandom rng)
        {
            var faces = new int[count];
            for (var i = 0; i < faces.Length; i++)
            {
                faces[i] = rng.Range(1, sides + 1);
            }

            var rerolls = Mathf.RoundToInt(maxLuckRerolls * luck);
            for (var r = 0; r < rerolls; r++)
            {
                var lowest = 0;
                for (var i = 1; i < faces.Length; i++)
                {
                    if (faces[i] < faces[lowest])
                    {
                        lowest = i;
                    }
                }

                faces[lowest] = Mathf.Max(faces[lowest], rng.Range(1, sides + 1));
            }

            return new DiceOutcome(faces, sides);
        }
    }
}
