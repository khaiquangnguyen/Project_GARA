using UnityEngine;

namespace GARA.Combat
{
    // Entry point for a run: hands the player/enemy roster to CombatManager
    // so it can generate CombatParticipants. Hardcoded to exactly 3 players
    // and 3 enemies for now — no run-generation/roster system exists yet.
    // Any slot can be player- or AI-controlled, whatever its side or class.
    public class RogueRunManager : MonoBehaviour
    {
        [SerializeField] private CombatManager combatManager;

        [Header("Players")]
        [SerializeField] private PlayerRosterSlot player1;
        [SerializeField] private PlayerRosterSlot player2;
        [SerializeField] private PlayerRosterSlot player3;

        [Header("Enemies")]
        [SerializeField] private EnemyRosterSlot enemy1;
        [SerializeField] private EnemyRosterSlot enemy2;
        [SerializeField] private EnemyRosterSlot enemy3;

        private void Start()
        {
            combatManager.GenerateParticipants(
                new[] { player1, player2, player3 },
                new[] { enemy1, enemy2, enemy3 });
        }
    }
}
