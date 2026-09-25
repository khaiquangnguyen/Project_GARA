using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Placeholder until real enemy AI exists — waits briefly, then randomly
    // picks one of the actor's authored skill cards (A/S/D/F) that it can
    // afford and that targets a single enemy (the only targeting this can
    // resolve yet), spends its AP/MP the same way the player's skill-card
    // flow does, then ends its turn once the whole swing (including the dash
    // back) is done. If nothing is affordable, the turn ends immediately with
    // a warning. Uses the same targeting-fade announcements a player's
    // special does, so the not-targeted and on-hit effects react identically
    // either way.
    public partial class CombatSceneManager
    {
        [Header("Enemy Turn")]
        [Tooltip("Seconds an enemy waits at the start of its turn before acting.")]
        [SerializeField] private float enemyTurnDelaySeconds = 0.5f;

        private IEnumerator PerformEnemyTurn(CombatParticipant actor, AttackExecutor actorExecutor, IReadOnlyList<CombatParticipant> livingTargets)
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

            if (livingTargets.Count == 0 || !TryPickAffordableEnemySkillCard(actor, out var card))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {actor.definition.displayName} has no affordable single-target skill card this turn — ending turn.", this);
                EndCombatPhase();
                yield break;
            }

            if (!actor.TrySpendResources(card.apCost, card.mpCost))
            {
                EndCombatPhase();
                yield break;
            }

            var target = livingTargets[Random.Range(0, livingTargets.Count)];

            AnnounceTargetingForSingleEnemyTarget(actor, target);
            actorExecutor.PlayAction(card.animationState, new ICombatTarget[] { target }, card.positionMode);
            yield return new WaitUntil(() => !actorExecutor.IsBusy);

            actorExecutor.ReturnToStandardPosition();
            yield return new WaitUntil(() => !actorExecutor.IsBusy);

            AnnounceTargetingClearedForEnemies(actor);
            EndCombatPhase();
        }

        // Collects every authored, affordable, single-enemy-target skill
        // card (slots 0-3, i.e. A/S/D/F) and returns one at random.
        private static bool TryPickAffordableEnemySkillCard(CombatParticipant actor, out SkillCardDefinition card)
        {
            var candidates = new List<SkillCardDefinition>();
            for (var slot = 0; slot < 4; slot++)
            {
                if (!actor.definition.TryGetSkillCard(slot, out var candidate))
                {
                    continue;
                }

                if (candidate.targetMode != SpecialTargetMode.OneEnemy)
                {
                    continue;
                }

                if (actor.currentAp < candidate.apCost || actor.currentMp < candidate.mpCost)
                {
                    continue;
                }

                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
            {
                card = null;
                return false;
            }

            card = candidates[Random.Range(0, candidates.Count)];
            return true;
        }
    }
}
