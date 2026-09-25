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

        // Raised by a state at its own "impact frame" (e.g. a Spine hit
        // event) so skill-card resolution can hook in without the state
        // knowing anything about skill cards. Null when no one is listening.
        public readonly Action OnImpact;

        public CharacterStateContext(IBattleQuery battle, ICombatTarget self, IReadOnlyList<ICombatTarget> targets = null, Action onImpact = null)
        {
            Battle = battle;
            Self = self;
            Targets = targets ?? Array.Empty<ICombatTarget>();
            OnImpact = onImpact;
        }
    }
}
