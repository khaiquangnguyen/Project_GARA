namespace GARA.Characters.Gambler
{
    // SkillPerformance.Details of a Gambler card. Outcome is null if aborted.
    public sealed class GambleReport
    {
        public CheatShakeReport Cheat { get; }

        public GambleOutcome Outcome { get; }

        // The cheat shake landed the exact count, so the roll was cheated.
        public bool Cheated => Cheat != null && Cheat.Cleared;

        public GambleReport(CheatShakeReport cheat, GambleOutcome outcome)
        {
            Cheat = cheat;
            Outcome = outcome;
        }
    }
}
