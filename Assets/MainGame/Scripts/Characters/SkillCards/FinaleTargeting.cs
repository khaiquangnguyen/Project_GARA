namespace GARA.Characters
{
    // Which of a live card's picked targets its finale hits.
    public enum FinaleTargeting
    {
        // Every pick.
        AllPicks,

        // One living pick, at random.
        OneRandomPick,

        // Only the picks an earlier step actually struck.
        PicksStruckBySteps
    }
}
