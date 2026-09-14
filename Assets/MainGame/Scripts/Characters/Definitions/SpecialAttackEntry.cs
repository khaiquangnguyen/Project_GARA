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
    // entries — array index 0-3 maps to A/S/D/F. OneEnemy uses whatever the
    // always-visible target selector currently points to; OneFriendly
    // always targets the first living ally; AllEnemy/AllFriendly hit every
    // living member of the relevant party.
    [Serializable]
    public struct SpecialAttackEntry
    {
        public CharacterState state;

        public SpecialTargetMode targetMode;

        public int apCost;

        public int mpCost;

        public ActionPositionMode positionMode;
    }
}
