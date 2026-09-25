namespace GARA.Characters
{
    // Mutable runtime state for one PassiveDefinition on one participant.
    // Lives on the participant's PassiveRuntimeSet, never on the asset —
    // the asset is shared across every character that has the passive.
    public abstract class PassiveRuntimeState
    {
        public PassiveDefinition Definition { get; internal set; }

        public virtual void Reset()
        {
        }
    }
}
