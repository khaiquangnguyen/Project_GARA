using System;

namespace GARA.Combat
{
    // Seam for enemy AI, which doesn't exist yet — CombatPhaseController
    // hands off a non-player actor's turn here instead of reading input for
    // it. Implementations must eventually call onTurnComplete exactly once.
    public interface IEnemyTurnController
    {
        void TakeTurn(CombatParticipant actor, Action onTurnComplete);
    }
}
