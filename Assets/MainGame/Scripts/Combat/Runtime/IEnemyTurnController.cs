using System;
using System.Collections.Generic;

namespace GARA.Combat
{
    // Seam for enemy AI, which doesn't exist yet — CombatPhaseController
    // hands off a non-player actor's turn here instead of reading input for
    // it. Implementations must eventually call onTurnComplete exactly once.
    // actorExecutor is the same AttackExecutor CombatPhaseController itself
    // drives for player actors; livingTargets is whoever's on the opposing
    // side of actor right now. announceTargeting/clearTargeting are
    // CombatPhaseController's own targeting-fade broadcasts (same ones a
    // player's basic attack uses) — call announceTargeting once a target is
    // actually chosen, and clearTargeting once the whole action (including
    // any dash back) is done, so an enemy's attack gets the exact same
    // not-targeted/on-hit visual treatment a player's does.
    public interface IEnemyTurnController
    {
        void TakeTurn(
            CombatParticipant actor,
            AttackExecutor actorExecutor,
            IReadOnlyList<CombatParticipant> livingTargets,
            Action<CombatParticipant> announceTargeting,
            Action clearTargeting,
            Action onTurnComplete);
    }
}
