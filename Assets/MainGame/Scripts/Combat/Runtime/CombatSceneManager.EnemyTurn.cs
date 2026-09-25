using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Placeholder until real enemy AI exists — waits briefly, then randomly
    // picks any card from the actor's combat loadout that has at least one
    // valid target, then ends its turn once the whole swing (including the
    // dash back) is done. Targeting follows the card's own mode, the same
    // rules a player's card uses except that the "one" modes pick at random
    // instead of reading a selector: One* hits one random living member of
    // its pool (enemies, friendlies, or every character), All* hits all of
    // them, and Multi* hits multiTargetCount random ones (repeats allowed or
    // not, per the mode). Enemies ignore AP/MP costs entirely — every card is free
    // for them. If no card qualifies, the turn ends immediately with a
    // warning. Uses the same targeting-fade announcements a player's card
    // does, so the not-targeted and on-hit effects react identically either
    // way.
    public partial class CombatSceneManager
    {
        [Header("Enemy Turn")]
        [Tooltip("Seconds an enemy waits at the start of its turn before acting.")]
        [SerializeField] private float enemyTurnDelaySeconds = 0.5f;

        private IEnumerator PerformEnemyTurn(CombatParticipant actor, AttackExecutor actorExecutor)
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

            AnnounceTargetingForSkillCard(actor, targets);
            RetreatUninvolved(actor, targets);
            _enemyActionInProgress = true;
            actorExecutor.PlayAction(actor.SkillCardStateOf(card), targets, card.positionMode);
            yield return new WaitUntil(() => !actorExecutor.IsBusy);
            _enemyActionInProgress = false;
            yield return new WaitForSeconds(card.endDelay);

            actorExecutor.ReturnToStandardPosition();
            yield return ReturnRetreated();
            yield return new WaitUntil(() => !actorExecutor.IsBusy);

            AnnounceTargetingClearedForSkillCard(actor);
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
