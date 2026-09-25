using NaughtyAttributes;
using UnityEngine;

namespace GARA.Combat
{
    // The one scene-side component of a combat scene (a clearing, a boss
    // arena, ...). It owns everything specific to this scene: where the three
    // left-side and three right-side character slots sit, the turn-by-turn
    // Combat Phase loop (player input and enemy turns), and the overlay that
    // presents it — so CombatManager itself never needs to know about any one
    // scene's layout or UI. Split across partial files by concern:
    //   .Phase     — the turn loop, player input and targeting broadcasts
    //   .EnemyTurn — placeholder enemy AI
    //   .LiveSkill — live skill cards that play steps during their minigame
    //   .Parry     — the player's parry window during an enemy's swing
    //   .Jump      — the player's jump (dodge) window during an enemy's swing
    //   .Overlay   — turn order, announcements and the minigame visual drivers
    public partial class CombatSceneManager : MonoBehaviour
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

        // How this scene stages the characters an action doesn't involve —
        // scene setup, never per card. "Not targeted" means every living
        // character on either side that's neither the actor nor a target.
        [Header("Combat Settings")]
        [Tooltip("Untargeted characters are dimmed while an action plays.")]
        [SerializeField] private bool fadeIfNotTargeted = true;

        [Tooltip("Untargeted characters shrink while an action plays.")]
        [SerializeField] private bool shrinkIfNotTargeted;

        [Tooltip("Untargeted characters step back to this scene's retreat slots while an action plays.")]
        [SerializeField] private bool retreatIfNotTargeted;

        [Header("Defense")]
        [Expandable]
        [SerializeField] private JumpSpec jumpSpec;

        [Expandable]
        [SerializeField] private ParrySpec parrySpec;

        private void Awake()
        {
            combatManager.Initialize(
                new[] { leftPosition1, leftPosition2, leftPosition3 },
                new[] { rightPosition1, rightPosition2, rightPosition3 },
                combatCharactersParent);

            AwakePhase();
        }

        private void OnEnable()
        {
            EnablePhase();
            EnableOverlay();
        }

        private void OnDisable()
        {
            DisablePhase();
            DisableOverlay();
        }
    }
}
