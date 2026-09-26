using GARA.ShakeBalance;

namespace GARA.Characters.Gambler
{
    // SkillPerformance.Details of a Gambler card. Outcome is null if aborted.
    public sealed class GambleReport
    {
        public readonly ShakeBalanceReport Balance;
        public readonly GambleOutcome Outcome;

        public GambleReport(ShakeBalanceReport balance, GambleOutcome outcome)
        {
            Balance = balance;
            Outcome = outcome;
        }
    }
}
