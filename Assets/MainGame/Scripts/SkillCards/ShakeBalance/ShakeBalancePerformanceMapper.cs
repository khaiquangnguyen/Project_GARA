using GARA.Characters;
using GARA.ShakeBalance;
using UnityEngine;

namespace GARA.SkillCards.ShakeBalance
{
    // Turns a raw ShakeBalanceReport into the generic SkillPerformance the
    // SkillCards framework understands, using the card's score shaping and
    // tiering thresholds. The report itself rides along as Details.
    public static class ShakeBalancePerformanceMapper
    {
        public static SkillPerformance Map(ShakeBalanceReport report, ShakeBalanceSkillCard card)
        {
            var score = Mathf.Clamp01(card.ScoreShaping.Evaluate(report.Result));
            var tier = report.WasAborted ? SkillPerformanceTier.Miss : card.TierFor(score);
            return new SkillPerformance(score, tier, report.WasAborted, report);
        }
    }
}
