using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Entry point for a run: hands the player/enemy roster to CombatManager
    // so it can generate CombatParticipants. Hardcoded to exactly 3 players
    // and 3 enemies for now — no run-generation/roster system exists yet.
    // Any slot can be player- or AI-controlled, whatever its side or class.
    public class RogueRunManager : MonoBehaviour
    {
        public const int SlotCount = 6;

        // Test-only character swaps by slot (0-2 players, 3-5 enemies), set
        // by the Character Manager before it reloads the scene.
        private static readonly Dictionary<int, CharacterDefinition> TestOverrides = new();

        [SerializeField] private CombatManager combatManager;

        [Header("Players")]
        [SerializeField] private PlayerRosterSlot player1;
        [SerializeField] private PlayerRosterSlot player2;
        [SerializeField] private PlayerRosterSlot player3;

        [Header("Enemies")]
        [SerializeField] private EnemyRosterSlot enemy1;
        [SerializeField] private EnemyRosterSlot enemy2;
        [SerializeField] private EnemyRosterSlot enemy3;

        // Domain reload is off, so statics would otherwise outlive play mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearTestOverrides()
        {
            TestOverrides.Clear();
        }

        public static void SetTestOverride(int slot, CharacterDefinition character)
        {
            TestOverrides[slot] = character;
        }

        // The character that spawns in this slot, test swaps included.
        public CharacterDefinition CharacterAt(int slot)
        {
            if (TestOverrides.TryGetValue(slot, out var character))
            {
                return character;
            }

            if (slot < BattleParty.Size)
            {
                return PlayerSlots()[slot].character;
            }

            var encounter = EnemySlots()[slot - BattleParty.Size].encounter;
            return encounter != null ? encounter.definition : null;
        }

        private void Start()
        {
            var players = PlayerSlots();
            var enemies = EnemySlots();
            ApplyTestOverrides(players, enemies);
            combatManager.GenerateParticipants(players, enemies);
        }

        private PlayerRosterSlot[] PlayerSlots()
        {
            return new[] { player1, player2, player3 };
        }

        private EnemyRosterSlot[] EnemySlots()
        {
            return new[] { enemy1, enemy2, enemy3 };
        }

        // An enemy swap keeps the slot's encounter settings (level etc.) on a
        // runtime copy, so the authored encounter asset is never touched.
        private static void ApplyTestOverrides(PlayerRosterSlot[] players, EnemyRosterSlot[] enemies)
        {
            foreach (var (slot, character) in TestOverrides)
            {
                if (slot < BattleParty.Size)
                {
                    players[slot].character = character;
                    continue;
                }

                ref var enemy = ref enemies[slot - BattleParty.Size];
                if (character == null)
                {
                    enemy.encounter = null;
                    continue;
                }

                var encounter = enemy.encounter != null ? Instantiate(enemy.encounter) : ScriptableObject.CreateInstance<EnemyEncounterData>();
                encounter.definition = character;
                enemy.encounter = encounter;
            }
        }
    }
}
