using System;
using UnityEngine;

namespace GARA.Characters
{
    // Authored thresholds that turn a raw 0-1 performance score into a
    // SkillPerformanceTier. Each threshold is the score below which the
    // *previous* tier still applies.
    [Serializable]
    public struct SkillPerformanceTiering
    {
        [Range(0, 1)] public float okThreshold;
        [Range(0, 1)] public float goodThreshold;
        [Range(0, 1)] public float perfectThreshold;

        public static SkillPerformanceTiering Default => new SkillPerformanceTiering
        {
            okThreshold = 0.35f,
            goodThreshold = 0.65f,
            perfectThreshold = 0.9f
        };

        public SkillPerformanceTier Evaluate(float score)
        {
            if (score < okThreshold)
            {
                return SkillPerformanceTier.Miss;
            }

            if (score < goodThreshold)
            {
                return SkillPerformanceTier.Ok;
            }

            if (score < perfectThreshold)
            {
                return SkillPerformanceTier.Good;
            }

            return SkillPerformanceTier.Perfect;
        }
    }
}
