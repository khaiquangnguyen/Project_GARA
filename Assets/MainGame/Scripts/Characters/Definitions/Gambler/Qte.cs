using System;
using GARA.Input;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The Gambler's timing prompts: a needle sweeps once around a ring; press
    // while it crosses the highlighted arc. Angles run clockwise from 12.
    [Serializable]
    public struct Qte
    {
        public InputToken token;

        [Tooltip("Seconds for the needle to sweep the full ring.")]
        [Min(0.1f)]
        public float sweepSeconds;

        [Tooltip("Width of the timing arc.")]
        [Range(5f, 180f)]
        public float windowDegrees;

        [Tooltip("Earliest angle the arc can start at.")]
        [Range(0f, 355f)]
        public float windowStartMin;

        [Tooltip("Latest angle the arc can start at; clamped so it ends by 360.")]
        [Range(0f, 355f)]
        public float windowStartMax;

        [Tooltip("Pause after each prompt resolves, before the next one; also how long the last result shows.")]
        [Min(0f)]
        public float secondsBetween;

        [Tooltip("Above 0: a new prompt starts every this many seconds, stacking behind any still up; a press resolves the oldest. 0: one at a time.")]
        [Min(0f)]
        public float overlapInterval;

        [Tooltip("Chance the AI-controlled Gambler lands each prompt.")]
        [Range(0f, 1f)]
        public float aiSuccessChance;

        public static Qte Default => new Qte
        {
            token = new InputToken(3),
            sweepSeconds = 1.2f,
            windowDegrees = 40f,
            windowStartMin = 120f,
            windowStartMax = 280f,
            secondsBetween = 0.25f,
            overlapInterval = 0f,
            aiSuccessChance = 0.5f
        };

        public float RollWindowStart()
        {
            var latest = 360f - windowDegrees;
            var min = Mathf.Clamp(Mathf.Min(windowStartMin, windowStartMax), 0f, latest);
            var max = Mathf.Clamp(Mathf.Max(windowStartMin, windowStartMax), min, latest);
            return UnityEngine.Random.Range(min, max);
        }
    }
}
