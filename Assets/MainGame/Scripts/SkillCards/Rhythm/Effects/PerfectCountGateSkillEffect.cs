using GARA.Characters;
using GARA.Rhythm;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // Picks which of several sub-effects to run based on how many Perfect
    // notes landed, clamped to the authored array's bounds. Falls back to
    // treating a Perfect tier as 4 perfects and anything else as 0 when no
    // rhythm report is available.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Rhythm/Perfect Count Gate")]
    public class PerfectCountGateSkillEffect : SkillEffectDefinition
    {
        [SerializeField]
        [Expandable]
        private SkillEffectDefinition[] tiersByPerfectCount;

        public override void Resolve(in SkillEffectContext context)
        {
            if (tiersByPerfectCount == null || tiersByPerfectCount.Length == 0)
            {
                return;
            }

            var perfectCount = context.Performance.TryGetDetails<RhythmCompletionReport>(out var report)
                ? report.PerfectCount
                : (context.Performance.Tier == SkillPerformanceTier.Perfect ? 4 : 0);

            var index = Mathf.Clamp(perfectCount, 0, tiersByPerfectCount.Length - 1);
            tiersByPerfectCount[index]?.Resolve(context);
        }
    }
}
