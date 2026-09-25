using System;

namespace GARA.Characters
{
    // Four authored values, one per SkillPerformanceTier, picked by For().
    // Used wherever a skill effect wants a discrete per-tier number instead
    // of a continuous StatScaling curve (e.g. a flat duration bonus per tier).
    [Serializable]
    public struct TierScaling
    {
        public float miss;
        public float ok;
        public float good;
        public float perfect;

        public float For(SkillPerformanceTier tier)
        {
            switch (tier)
            {
                case SkillPerformanceTier.Miss:
                    return miss;

                case SkillPerformanceTier.Ok:
                    return ok;

                case SkillPerformanceTier.Good:
                    return good;

                case SkillPerformanceTier.Perfect:
                    return perfect;

                default:
                    return 0f;
            }
        }
    }
}
