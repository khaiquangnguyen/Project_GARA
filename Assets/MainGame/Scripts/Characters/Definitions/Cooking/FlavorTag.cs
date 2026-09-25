using System;

namespace GARA.Characters
{
    // Flavors a dish (RecipeSkillCard) or a target (PalateProfile) can carry.
    // Umami is deliberately hidden from players — see FlavorTags.Visible —
    // it always contributes to fullness but never shows up as a favorite or
    // in dish-vs-palate match counting.
    [Flags]
    public enum FlavorTag
    {
        None = 0,
        Spicy = 1 << 0,
        Sweet = 1 << 1,
        Sour = 1 << 2,
        Bitter = 1 << 3,
        Umami = 1 << 4
    }
}
