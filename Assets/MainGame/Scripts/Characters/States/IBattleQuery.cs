using System.Collections.Generic;

namespace GARA.Characters
{
    // Read-only query surface over a battle, scoped to whoever is acting.
    // Implemented by a GARA.Combat-side adapter over BattleContext. Kept
    // minimal — damage + faction queries only, no status effects/turn
    // manipulation, since neither has a settled API elsewhere yet.
    public interface IBattleQuery
    {
        ICombatTarget Self { get; }
        IEnumerable<ICombatTarget> Allies { get; }
        IEnumerable<ICombatTarget> Enemies { get; }

        // Battle-wide, shared by every participant's query.
        NoirWorld NoirWorld { get; }
    }
}
