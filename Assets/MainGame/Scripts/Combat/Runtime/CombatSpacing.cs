using UnityEngine;

namespace GARA.Combat
{
    // "In front of the enemy": range units along X from the target, on the
    // side the attacker's home spot is on — not where it stands now, so an
    // attacker already among the enemies (e.g. between two targets of one
    // card) still lands on its own side.
    public static class CombatSpacing
    {
        public static Vector3 PositionInFrontOfEnemy(Vector3 attackerHome, CombatParticipant enemy, float range)
        {
            var enemyPosition = enemy.SceneTransform.position;
            var sideSign = Mathf.Sign(attackerHome.x - enemyPosition.x);

            if (sideSign == 0f)
            {
                sideSign = 1f;
            }

            return enemyPosition + new Vector3(sideSign * range, 0f, 0f);
        }
    }
}
