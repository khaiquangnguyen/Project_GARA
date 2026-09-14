using GARA.Characters;

namespace GARA.Combat
{
    public enum ComboResolutionKind
    {
        BasicAttack,
        Finisher,
        None
    }

    // What a single buffered input resolved to: either that input's own
    // basic attack, a combo finisher (if the running sequence just matched
    // one), or nothing. State is the asset-side reference (the component as
    // authored on the character's prefab) — resolving it to the live
    // instance on a spawned participant is CombatParticipant's job.
    public readonly struct ComboResolution
    {
        public readonly ComboResolutionKind kind;
        public readonly CharacterState state;

        private ComboResolution(ComboResolutionKind kind, CharacterState state)
        {
            this.kind = kind;
            this.state = state;
        }

        public static ComboResolution Basic(CharacterState state)
        {
            return new ComboResolution(ComboResolutionKind.BasicAttack, state);
        }

        public static ComboResolution Finisher(CharacterState state)
        {
            return new ComboResolution(ComboResolutionKind.Finisher, state);
        }

        public static ComboResolution None()
        {
            return new ComboResolution(ComboResolutionKind.None, null);
        }
    }
}
