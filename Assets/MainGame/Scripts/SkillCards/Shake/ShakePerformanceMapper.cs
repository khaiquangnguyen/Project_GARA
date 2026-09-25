using GARA.Characters;
using GARA.Shake;
using UnityEngine;

namespace GARA.SkillCards.Shake
{
    // Turns a raw ShakeInputReport into a generic SkillPerformance, applying
    // the card's target/penalty/shaping configuration.
    public static class ShakePerformanceMapper
    {
        public static SkillPerformance Map(ShakeInputReport report, ShakeSkillCard card, bool wasAborted)
        {
            var progress = ComputeProgress(report.CompletedPairs, report.WrongPresses, card.TargetPairs, card.WrongPressPenalty);
            var shapedScore = Mathf.Clamp01(card.ScoreShaping.Evaluate(progress));
            var tier = wasAborted ? SkillPerformanceTier.Miss : card.TierFor(shapedScore);
            var overflow = Mathf.Max(0, report.CompletedPairs - card.TargetPairs);
            var reachedTarget = report.CompletedPairs >= card.TargetPairs;

            var shakeSkillReport = new ShakeSkillReport(report, card.TargetPairs, overflow, progress, reachedTarget);

            return new SkillPerformance(shapedScore, tier, wasAborted, shakeSkillReport);
        }

        public static float ComputeProgress(int completedPairs, int wrongPresses, int targetPairs, float wrongPressPenalty)
        {
            var raw = completedPairs - wrongPressPenalty * wrongPresses;
            return Mathf.Clamp01(targetPairs > 0 ? raw / targetPairs : 0f);
        }
    }
}
