using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Entry point for a run: hands the player/enemy roster to CombatManager
    // so it can generate CombatParticipants. Hardcoded to exactly 3 players
    // and 3 enemies for now — no run-generation/roster system exists yet.
    public class RogueRunManager : MonoBehaviour
    {
        [SerializeField] private CombatManager combatManager;

        [Header("Players")]
        [SerializeField] private CharacterDefinition player1;
        [SerializeField] private CharacterDefinition player2;
        [SerializeField] private CharacterDefinition player3;

        [Header("Enemies")]
        [SerializeField] private EnemyEncounterData enemy1;
        [SerializeField] private EnemyEncounterData enemy2;
        [SerializeField] private EnemyEncounterData enemy3;

        private void Start()
        {
            combatManager.GenerateParticipants(
                new[] { player1, player2, player3 },
                new[] { enemy1, enemy2, enemy3 });
        }
    }
}
