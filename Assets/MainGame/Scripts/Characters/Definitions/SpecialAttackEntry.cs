using System;

namespace GARA.Characters
{
    public enum SpecialTargetMode
    {
        OneEnemy,
        AllEnemy,
        OneFriendly,
        AllFriendly
    }

    // Authored per character on CharacterDefinition.specials, capped at 4
    // entries — array index 0-3 maps to A/S/D/F. OneEnemy/OneFriendly
    // specials open interactive target selection (Left/Right + Enter to
    // confirm) once chosen; AllEnemy/AllFriendly bypass selection entirely
    // and hit every living member of the relevant party.
    [Serializable]
    public struct SpecialAttackEntry
    {
        public CharacterState state;

        public SpecialTargetMode targetMode;

        public int apCost;

        public int mpCost;
    }
}
