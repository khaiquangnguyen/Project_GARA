using System;
using System.Collections.Generic;

namespace GARA.Characters
{
    // Slot -> equipped item ID. Structure-only stub: no item database, no
    // stat contribution yet. The real equipment system fills this in and
    // implements IStatModifierSource against it.
    [Serializable]
    public class EquipmentLoadout
    {
        public Dictionary<EquipmentSlotType, string> EquippedItemIds = new();
    }
}
