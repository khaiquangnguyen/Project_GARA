using System;
using GARA.Input;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The cheat minigame a Gambler card plays: alternate left/right exactly
    // a random required count, within required × secondsPerInput.
    [Serializable]
    public struct CheatShake
    {
        public InputToken leftToken;

        public InputToken rightToken;

        [Tooltip("Ends the run early.")]
        public InputToken doneToken;

        [Min(1)]
        public int requiredMin;

        [Min(1)]
        public int requiredMax;

        [Tooltip("Time limit per required input; the timer starts on the first press.")]
        [Min(0.05f)]
        public float secondsPerInput;

        [Tooltip("Chance the AI-controlled Gambler lands the exact count.")]
        [Range(0f, 1f)]
        public float aiSuccessChance;

        public static CheatShake Default => new CheatShake
        {
            leftToken = new InputToken(2),
            rightToken = new InputToken(4),
            doneToken = new InputToken(3),
            requiredMin = 5,
            requiredMax = 10,
            secondsPerInput = 0.2f,
            aiSuccessChance = 0.5f
        };

        public int RollRequired()
        {
            var min = Mathf.Max(1, Mathf.Min(requiredMin, requiredMax));
            var max = Mathf.Max(min, Mathf.Max(requiredMin, requiredMax));
            return UnityEngine.Random.Range(min, max + 1);
        }
    }
}
