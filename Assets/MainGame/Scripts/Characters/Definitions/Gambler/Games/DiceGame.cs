using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Rolls count dice. Cheat: every die below cheatRerollBelow is rerolled
    // once.
    [Serializable]
    public class DiceGame : GamblingGame<DiceOutcome>
    {
        [Min(1)]
        public int count = 2;

        [Min(2)]
        public int sides = 6;

        [Tooltip("Cheat: faces below this are rerolled once. 1 = no cheat.")]
        [Min(1)]
        public int cheatRerollBelow = 3;

        protected override DiceOutcome RollTyped(bool cheated, IGambleRandom rng)
        {
            var faces = new int[count];
            for (var i = 0; i < faces.Length; i++)
            {
                faces[i] = rng.Range(1, sides + 1);
                if (cheated && faces[i] < cheatRerollBelow)
                {
                    faces[i] = rng.Range(1, sides + 1);
                }
            }

            return new DiceOutcome(faces, sides);
        }
    }
}
