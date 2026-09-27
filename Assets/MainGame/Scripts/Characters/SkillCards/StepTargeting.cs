namespace GARA.Characters
{
    // Which of a live card's picked targets each step (not the finale) hits.
    public enum StepTargeting
    {
        // Every pick.
        AllPicks,

        // One pick per step, in turn (step 1 the first, step 2 the
        // second...), wrapping when fewer were picked than there are steps.
        TakeTurns,

        // One living pick per step, at random.
        RandomLivingPick
    }
}
