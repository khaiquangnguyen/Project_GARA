using System.Collections.Generic;
using System.Linq;

namespace GARA.Characters
{
    // How statuses narrow what a target-picking card (or random pick) can
    // choose. Hit-all cards skip this entirely.
    public static class TargetRestrictions
    {
        // Drops untargetable opponents; if any opponent left is taunting and
        // the pool is opponents only, those taunters are the only choices
        // (narrowedByTaunt: multi picks then repeat on them to keep count).
        public static List<T> Pickable<T>(ICombatTarget picker, IEnumerable<T> pool, bool opponentsOnly, out bool narrowedByTaunt)
            where T : ICombatTarget
        {
            var pickable = pool.Where(target => !(target.IsUntargetable && IsOpponent(picker, target))).ToList();
            var taunters = opponentsOnly ? pickable.Where(target => target.IsTaunting && IsOpponent(picker, target)).ToList() : null;
            narrowedByTaunt = taunters != null && taunters.Count > 0;
            return narrowedByTaunt ? taunters : pickable;
        }

        private static bool IsOpponent(ICombatTarget picker, ICombatTarget target)
        {
            return picker == null || target.Faction != picker.Faction;
        }
    }
}
