using System;
using System.Collections;
using UnityEngine;

namespace GARA.Combat
{
    // Placeholder until real enemy AI exists — makes no decisions, deals no
    // damage, just waits so the turn order visibly advances past enemies.
    public class StubEnemyTurnController : MonoBehaviour, IEnemyTurnController
    {
        [SerializeField] private float delaySeconds = 0.5f;

        public void TakeTurn(CombatParticipant actor, Action onTurnComplete)
        {
            StartCoroutine(WaitThenComplete(onTurnComplete));
        }

        private IEnumerator WaitThenComplete(Action onTurnComplete)
        {
            yield return new WaitForSeconds(delaySeconds);
            onTurnComplete();
        }
    }
}
