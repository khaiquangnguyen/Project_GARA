using System;
using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Placeholder until real enemy AI exists — waits briefly, then randomly
    // performs Atk1 or Atk2 (never Atk3/a combo finisher; no combo logic
    // here) against a randomly chosen living target, then ends its turn
    // once the whole swing (including the dash back) is done. Uses the same
    // targeting-fade announce/clear calls a player's basic attack does, so
    // the not-targeted and on-hit effects react identically either way.
    public class StubEnemyTurnController : MonoBehaviour, IEnemyTurnController
    {
        [SerializeField] private float delaySeconds = 0.5f;

        public void TakeTurn(
            CombatParticipant actor,
            AttackExecutor actorExecutor,
            IReadOnlyList<CombatParticipant> livingTargets,
            Action<CombatParticipant> announceTargeting,
            Action clearTargeting,
            Action onTurnComplete)
        {
            StartCoroutine(PerformTurn(actor, actorExecutor, livingTargets, announceTargeting, clearTargeting, onTurnComplete));
        }

        private IEnumerator PerformTurn(
            CombatParticipant actor,
            AttackExecutor actorExecutor,
            IReadOnlyList<CombatParticipant> livingTargets,
            Action<CombatParticipant> announceTargeting,
            Action clearTargeting,
            Action onTurnComplete)
        {
            yield return new WaitForSeconds(delaySeconds);

            var input = UnityEngine.Random.value < 0.5f ? AttackInput.Atk1 : AttackInput.Atk2;

            if (livingTargets.Count == 0
                || !actor.definition.TryGetBasicAttack(input, out var entry)
                || !actor.TrySpendAp(entry.apCost))
            {
                onTurnComplete();
                yield break;
            }

            var target = livingTargets[UnityEngine.Random.Range(0, livingTargets.Count)];

            announceTargeting(target);
            actorExecutor.PlayAction(entry.state, new ICombatTarget[] { target }, entry.positionMode);
            yield return new WaitUntil(() => !actorExecutor.IsBusy);

            actorExecutor.ReturnToStandardPosition();
            yield return new WaitUntil(() => !actorExecutor.IsBusy);

            clearTargeting();
            onTurnComplete();
        }
    }
}
