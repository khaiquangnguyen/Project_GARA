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
    // positionMode carries the originating BasicAttackEntry/ComboDefinition's
    // own authored position mode through to whoever plays the resolved state.
    public readonly struct ComboResolution
    {
        public readonly ComboResolutionKind kind;
        public readonly CharacterState state;
        public readonly ActionPositionMode positionMode;

        private ComboResolution(ComboResolutionKind kind, CharacterState state, ActionPositionMode positionMode)
        {
            this.kind = kind;
            this.state = state;
            this.positionMode = positionMode;
        }

        public static ComboResolution Basic(CharacterState state, ActionPositionMode positionMode)
        {
            return new ComboResolution(ComboResolutionKind.BasicAttack, state, positionMode);
        }

        public static ComboResolution Finisher(CharacterState state, ActionPositionMode positionMode)
        {
            return new ComboResolution(ComboResolutionKind.Finisher, state, positionMode);
        }

        public static ComboResolution None()
        {
            return new ComboResolution(ComboResolutionKind.None, null, ActionPositionMode.StayAtOriginalPosition);
        }
    }
}
