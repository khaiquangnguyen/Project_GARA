using UnityEngine;

namespace GARA.Combat
{
    // Scene-specific slot placement. Each combat scene (a clearing, a boss
    // arena, ...) gets its own CombatSceneManager wiring up where the three
    // left-side and three right-side character slots actually sit, so
    // CombatManager itself never needs to know about any one scene's layout.
    public class CombatSceneManager : MonoBehaviour
    {
        [SerializeField] private CombatManager combatManager;

        [Header("Left Side")]
        [SerializeField] private Transform leftPosition1;
        [SerializeField] private Transform leftPosition2;
        [SerializeField] private Transform leftPosition3;

        [Header("Right Side")]
        [SerializeField] private Transform rightPosition1;
        [SerializeField] private Transform rightPosition2;
        [SerializeField] private Transform rightPosition3;

        [Header("Spawning")]
        [Tooltip("Spawned character instances are parented here.")]
        [SerializeField] private Transform combatCharactersParent;

        private void Awake()
        {
            combatManager.Initialize(
                new[] { leftPosition1, leftPosition2, leftPosition3 },
                new[] { rightPosition1, rightPosition2, rightPosition3 },
                combatCharactersParent);
        }
    }
}
