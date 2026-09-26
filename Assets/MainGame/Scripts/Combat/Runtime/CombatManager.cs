using System;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    public class CombatManager : MonoBehaviour
    {
        [SerializeField] private CombatSceneManager combatSceneManager;

        private BattleContext _battle;
        private Transform[] _leftSlotAnchors;
        private Transform[] _rightSlotAnchors;
        private Transform _combatCharactersParent;
        private readonly Dictionary<CombatParticipant, AttackExecutor> _executors = new();

        // Awake, not Start — callers (e.g. RogueRunManager) generate
        // participants from their own Start, and Unity only guarantees
        // Awake-before-any-Start ordering across scripts, not Start order.
        private void Awake()
        {
            _battle = new BattleContext();
        }

        // Hardcoded, minimal roster -> participants path — no save/roster
        // system feeds this yet, so each player gets a fresh ManagedCharacter
        // built on the spot from its CharacterDefinition. Players spawn on
        // the left, enemies on the right — each instantiated from its own
        // CharacterDefinition's prefab (the definition lives on the prefab
        // root, so definition.gameObject is that prefab), placed feet-first
        // at its slot anchor, then bound so its live CharacterStates and
        // AttackExecutor are usable.
        public void GenerateParticipants(PlayerRosterSlot[] players, EnemyRosterSlot[] enemies)
        {
            for (var i = 0; i < players.Length && i < BattleParty.Size; i++)
            {
                var definition = players[i].character;
                if (definition == null)
                {
                    continue;
                }

                var managedCharacter = new ManagedCharacter(Guid.NewGuid().ToString(), definition.characterId);
                var participant = CombatParticipant.FromManagedCharacter(managedCharacter, definition, FactionTag.Player);
                participant.playerControlled = players[i].IsPlayerControlled;
                _battle.playerParty.Slots[i].occupant = participant;

                SpawnAndBind(participant, definition.gameObject, isLeft: true, i);
            }

            for (var i = 0; i < enemies.Length && i < BattleParty.Size; i++)
            {
                var encounter = enemies[i].encounter;
                if (encounter == null)
                {
                    continue;
                }

                var participant = CombatParticipant.FromEnemyEncounter(encounter, FactionTag.Enemy);
                participant.playerControlled = enemies[i].IsPlayerControlled;
                participant.charmedToPlayerSide = enemies[i].charmedToPlayerSide;
                _battle.enemyParty.Slots[i].occupant = participant;

                SpawnAndBind(participant, encounter.definition.gameObject, isLeft: false, i);
            }

            combatSceneManager.BeginBattle(_battle, _executors);
        }

        private void SpawnAndBind(CombatParticipant participant, GameObject prefab, bool isLeft, int slotIndex)
        {
            var instance = Instantiate(prefab, _combatCharactersParent);

            if (isLeft)
            {
                PlaceAtLeftSlot(instance, slotIndex);
            }
            else
            {
                PlaceAtRightSlot(instance, slotIndex);
            }

            participant.BindToSceneInstance(instance);

            var executor = instance.GetComponent<AttackExecutor>();
            if (executor != null)
            {
                executor.Initialize(participant, _battle.QueryFor(participant));
                _executors[participant] = executor;
            }
        }

        // Called by this scene's CombatSceneManager so CombatManager itself
        // stays scene-agnostic — a different combat scene can wire up
        // entirely different physical slot placements (and spawn parent)
        // without touching this class.
        public void Initialize(Transform[] leftSlotAnchors, Transform[] rightSlotAnchors, Transform combatCharactersParent)
        {
            _leftSlotAnchors = leftSlotAnchors;
            _rightSlotAnchors = rightSlotAnchors;
            _combatCharactersParent = combatCharactersParent;
        }

        // Places an already-instantiated character root at the given slot,
        // aligning the character's local origin (its feet, by Spine rig
        // convention) with the slot anchor.
        public void PlaceAtLeftSlot(GameObject characterRoot, int slotIndex)
        {
            PlaceAt(characterRoot, _leftSlotAnchors, slotIndex);
        }

        public void PlaceAtRightSlot(GameObject characterRoot, int slotIndex)
        {
            PlaceAt(characterRoot, _rightSlotAnchors, slotIndex);
        }

        private static void PlaceAt(GameObject characterRoot, Transform[] anchors, int slotIndex)
        {
            var anchor = anchors[slotIndex];
            characterRoot.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }
    }
}
