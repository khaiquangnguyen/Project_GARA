using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Placeholder AI for any AI-controlled character, on either side: waits
    // briefly, plays a random loadout card that has a valid target (random
    // picks for the "one"/Multi* modes, costs ignored) the same way a
    // player's card plays, then ends the turn once it's resolved.
    public partial class CombatSceneManager
    {
        [Header("Enemy Turn")]
        [Tooltip("Seconds an enemy waits at the start of its turn before acting.")]
        [SerializeField] private float enemyTurnDelaySeconds = 0.5f;

        private IEnumerator PerformAiTurn(CombatParticipant actor, AttackExecutor actorExecutor)
        {
            yield return new WaitForSeconds(enemyTurnDelaySeconds);

            // Defensive guard: BeginPhaseForCurrentActor already routes a
            // stunned (food-coma'd) actor to SkipStunnedTurn instead, but
            // don't rely solely on that — never let a stunned actor act.
            if (actor.IsStunned)
            {
                EndCombatPhase();
                yield break;
            }

            if (!TryPickEnemySkillCard(actor, out var card, out var targets))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {actor.definition.displayName} has no usable skill card this turn — ending turn.", this);
                EndCombatPhase();
                yield break;
            }

            // Same path as a player's card, minus the cost.
            _aiActionInProgress = true;
            StartSkillCard(card, targets);
            yield return new WaitUntil(() => _phaseActionState == PhaseActionState.Regular);
            EndCombatPhase();
        }

        // Collects every loadout card that has at least one valid target
        // right now and returns one at random, along with its targets.
        private bool TryPickEnemySkillCard(CombatParticipant actor, out SkillCardDefinition card, out IReadOnlyList<ICombatTarget> targets)
        {
            var candidates = new List<(SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets)>();
            foreach (var candidate in actor.SkillCards)
            {
                if (candidate != null && TryResolveEnemyTargets(actor, candidate, out var candidateTargets))
                {
                    candidates.Add((candidate, candidateTargets));
                }
            }

            if (candidates.Count == 0)
            {
                card = null;
                targets = null;
                return false;
            }

            (card, targets) = candidates[Random.Range(0, candidates.Count)];
            return true;
        }

        private bool TryResolveEnemyTargets(CombatParticipant actor, SkillCardDefinition card, out IReadOnlyList<ICombatTarget> targets)
        {
            var pool = LivingPoolOf(actor, card.targetMode.GetPool());
            targets = card.targetMode.PickRandomTargets(pool, card.multiTargetCount).Cast<ICombatTarget>().ToArray();
            return targets.Count > 0;
        }
    }
}
