using GARA.Characters;
using GARA.InputSets;

namespace GARA.SkillCards.InputSets
{
    // Turns a raw InputSetCompletionReport into the generic SkillPerformance
    // the SkillCards framework understands, using the card's authored
    // InputSetScoreModel and tiering thresholds.
    public static class InputSetPerformanceMapper
    {
        public static SkillPerformance Map(InputSetCompletionReport report, InputSetSkillCard card)
        {
            var score = card.ScoreModel.Evaluate(report);
            var tier = report.WasAborted ? SkillPerformanceTier.Miss : card.TierFor(score);
            return new SkillPerformance(score, tier, report.WasAborted, report);
        }
    }
}
