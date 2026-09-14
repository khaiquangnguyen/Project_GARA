namespace GARA.Combat
{
    // The Combat Phase's current top-level mode for the acting player
    // character: Regular (free to pick any action) or ChainedAction
    // (locked into whichever multi-input action currently owns the phase's
    // one chained-action slot — basic attack today, potentially a chained
    // special later). Deliberately doesn't track *which* chain is active —
    // only one can be active at a time, and each driver tracks its own
    // "is this my chain" locally (see CombatPhaseController.BasicAttack.cs).
    public enum PhaseActionState
    {
        Regular,
        ChainedAction
    }
}
