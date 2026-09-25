using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // Pure mapping from a finished rhythm run to the generic SkillPerformance
    // the skill-card framework scales effects against. Kept static/free of
    // Unity lifecycle so it's unit-testable on its own.
    public static class RhythmPerformanceMapper
    {
        public static SkillPerformance Map(RhythmCompletionReport report, RhythmSkillCard card)
        {
            var baseScore = card.ScoreSource switch
            {
                RhythmScoreSource.Accuracy => report.AccuracyScore,
                RhythmScoreSource.Completion => report.CompletionRate,
                RhythmScoreSource.Blend => (report.AccuracyScore + report.CompletionRate) / 2f,
                _ => report.AccuracyScore
            };

            var penalized = Mathf.Clamp01(baseScore - report.StrayPresses * card.StrayPressPenalty);

            var tier = (report.CompletionRate < card.CompletionGate || report.WasAborted)
                ? SkillPerformanceTier.Miss
                : card.TierFor(penalized);

            return new SkillPerformance(penalized, tier, report.WasAborted, report);
        }
    }
}
