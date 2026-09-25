namespace GARA.Combat
{
    // What CombatSceneManager is doing with the actor's current turn.
    // Regular is the default — free to choose a skill card, cycle targets,
    // or end the phase. SkillCardTargeting is a card waiting on
    // its targets to be picked — only target cycling/picking/cancelling
    // apply there. SkillCardInput/SkillCardResolving bracket one skill-card
    // use: input while its minigame is running, resolving while
    // its animation/effects play out — both gate input handling back to a
    // no-op until the flow returns to Regular.
    public enum PhaseActionState
    {
        Regular,
        SkillCardTargeting,
        SkillCardInput,
        SkillCardResolving
    }
}
