using System;
using UnityEngine;

namespace GARA.Combat
{
    // One beam of an EncoreSpotlightEffect: its colour, where its light hangs
    // and how it sweeps onto its target.
    [Serializable]
    public class EncoreBeam
    {
        [ColorUsage(false, true)]
        public Color color = Color.white;

        [Tooltip("World units beside the target the light hangs, negative for left.")]
        public float sourceOffset;

        [Tooltip("World units beside the target it starts aimed at, then sweeps in from.")]
        public float sweepFrom;

        [Tooltip("Radians into its sway it starts, so the beams don't sway in step.")]
        public float swayPhase;
    }
}
