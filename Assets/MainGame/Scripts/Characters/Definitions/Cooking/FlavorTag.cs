using System;

namespace GARA.Characters
{
    // Flavors a dish (ChefSkillCard) or a target (PalateProfile) can carry.
    // Umami is dish-only: enemies can't favor it, and it pairs with any
    // favorite flavor (see MasterchefPassive).
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
