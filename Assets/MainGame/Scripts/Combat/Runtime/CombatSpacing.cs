using UnityEngine;

namespace GARA.Combat
{
    // Placeholder "in front of the enemy" spatial math — a fixed X offset
    // from the target's position, on whichever side the attacker is
    // currently standing (so it works for either party without hardcoding
    // "left side"/"right side").
    public static class CombatSpacing
    {
        public const float FrontOffsetX = 1f;

        public static Vector3 PositionInFrontOfEnemy(CombatParticipant attacker, CombatParticipant enemy)
        {
            var enemyPosition = enemy.SceneTransform.position;
            var attackerPosition = attacker.SceneTransform.position;
            var sideSign = Mathf.Sign(attackerPosition.x - enemyPosition.x);

            if (sideSign == 0f)
            {
                sideSign = 1f;
            }

            return enemyPosition + new Vector3(sideSign * FrontOffsetX, 0f, 0f);
        }
    }
}
