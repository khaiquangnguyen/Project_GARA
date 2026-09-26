using UnityEngine;

namespace GARA.Characters
{
    // A card whose minigame scores itself and tiers that score with its own
    // authored thresholds.
    public abstract class TieredSkillCard : SkillCardDefinition
    {
        [SerializeField]
        private SkillPerformanceTiering tiering = SkillPerformanceTiering.Default;

        public SkillPerformanceTier TierFor(float score)
        {
            return tiering.Evaluate(score);
        }
    }
}
