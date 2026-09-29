namespace GARA.Characters.Gambler
{
    // SkillPerformance.Details of a Gambler card. Outcome is null if aborted.
    public sealed class GambleReport
    {
        public QteReport Qte { get; }

        public GambleOutcome Outcome { get; }

        // The QTE was hit, so the roll was cheated (single-roll cards).
        public bool Cheated => Qte != null && Qte.AllHit;

        public GambleReport(QteReport qte, GambleOutcome outcome)
        {
            Qte = qte;
            Outcome = outcome;
        }
    }
}
