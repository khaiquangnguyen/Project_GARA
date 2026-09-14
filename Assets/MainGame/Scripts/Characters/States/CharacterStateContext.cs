using System;
using System.Collections.Generic;

namespace GARA.Characters
{
    // Payload handed to CharacterState.Enter. A plain struct rather than an
    // interface so it can grow later without breaking Enter's signature.
    public readonly struct CharacterStateContext
    {
        public readonly IBattleQuery Battle;
        public readonly ICombatTarget Self;
        public readonly IReadOnlyList<ICombatTarget> Targets;

        public CharacterStateContext(IBattleQuery battle, ICombatTarget self, IReadOnlyList<ICombatTarget> targets = null)
        {
            Battle = battle;
            Self = self;
            Targets = targets ?? Array.Empty<ICombatTarget>();
        }
    }
}
