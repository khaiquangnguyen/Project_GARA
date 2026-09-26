using GARA.Characters;
using GARA.Rhythm;

namespace GARA.SkillCards.Rhythm
{
    // Maps a rhythm run to the generic SkillPerformance: score is accuracy,
    // tiered by the actor's tiering (Miss if aborted). Effects that need
    // more read the report itself, carried as Details.
    public static class RhythmPerformanceMapper
    {
        public static SkillPerformance Map(RhythmCompletionReport report, SkillPerformanceTiering tiering)
        {
            var score = report.AccuracyScore;
            var tier = report.WasAborted ? SkillPerformanceTier.Miss : tiering.Evaluate(score);
            return new SkillPerformance(score, tier, report.WasAborted, report);
        }
    }
}
