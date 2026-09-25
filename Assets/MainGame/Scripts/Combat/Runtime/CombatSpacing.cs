using UnityEngine;

namespace GARA.Combat
{
    // "In front of the enemy": range units along X from the target, on
    // whichever side the attacker is currently standing.
    public static class CombatSpacing
    {
        public static Vector3 PositionInFrontOfEnemy(CombatParticipant attacker, CombatParticipant enemy, float range)
        {
            var enemyPosition = enemy.SceneTransform.position;
            var attackerPosition = attacker.SceneTransform.position;
            var sideSign = Mathf.Sign(attackerPosition.x - enemyPosition.x);

            if (sideSign == 0f)
            {
                sideSign = 1f;
            }

            return enemyPosition + new Vector3(sideSign * range, 0f, 0f);
        }
    }
}
