namespace GARA.Characters.Gambler
{
    // Result of one GamblingGameDefinition.Resolve call, before/after a rig's
    // AfterResolve has had a chance to reshape it. GameDetails is the
    // game-specific payload (CoinFlipDetails, DiceRollDetails, ...).
    public readonly struct GambleOutcome
    {
        public readonly GambleResult Result;
        public readonly float Margin;
        public readonly float FinalSuccessChance;
        public readonly float BackfireDamageFraction;
        public readonly object GameDetails;

        public GambleOutcome(GambleResult result, float margin, float finalSuccessChance, float backfireDamageFraction, object gameDetails)
        {
            Result = result;
            Margin = margin;
            FinalSuccessChance = finalSuccessChance;
            BackfireDamageFraction = backfireDamageFraction;
            GameDetails = gameDetails;
        }
    }
}
