using GARA.Rhythm;

namespace GARA.SkillCards.Rhythm
{
    // What a step's notes must achieve for a GatedRhythmStepEffect to apply.
    public enum StepGate
    {
        Always,
        AnyNoteHit,
        AllNotesHit,
        AllNotesPerfect
    }

    public static class StepGateExtensions
    {
        public static bool Passes(this StepGate gate, RhythmCompletionReport report)
        {
            return gate switch
            {
                StepGate.AnyNoteHit => report.HitNotes > 0,
                StepGate.AllNotesHit => report.TotalNotes > 0 && report.MissCount == 0,
                StepGate.AllNotesPerfect => report.TotalNotes > 0 && report.PerfectCount == report.TotalNotes,
                _ => true
            };
        }
    }
}
