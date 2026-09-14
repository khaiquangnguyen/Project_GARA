using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Lives on the root of a character's battle prefab, alongside its Spine
    // rig and every CharacterState it authors attacks/other actions against.
    // The prefab is the whole unit of authoring now — no separate SO asset.
    public class CharacterDefinition : MonoBehaviour
    {
        [Tooltip(
            "Stable ID used for save data and lookups. Must not change once referenced by saved ManagedCharacters.")]
        public string characterId;

        public string displayName;

        public Sprite portrait;

        public FactionTag faction;

        [Header("Stats")]
        public BaseStatValues baseStats;

        [Header("Abilities")]
        public string[] innateAbilityIds = Array.Empty<string>();

        public LevelUnlockEntry[] levelUnlockedAbilities = Array.Empty<LevelUnlockEntry>();

        [Header("Equipment (placeholder — full system designed later)")]
        public EquipmentSlotType[] allowedEquipmentSlots = Array.Empty<EquipmentSlotType>();

        [Header("States")]
        [Tooltip("Entered automatically once a spawned participant is initialized for combat — always Idle today.")]
        public CharacterState defaultState;

        [Header("Basic Attacks & Combos")]
        public BasicAttackEntry[] basicAttacks = Array.Empty<BasicAttackEntry>();

        public ComboDefinition[] combos = Array.Empty<ComboDefinition>();

        [Header("Specials")]
        [Tooltip("Up to 4 entries — array index 0-3 maps to A/S/D/F.")]
        [SerializeField]
        private SpecialAttackEntry[] specials = Array.Empty<SpecialAttackEntry>();

        public IReadOnlyList<SpecialAttackEntry> Specials => specials;

        private const int MaxSpecials = 4;

        private void OnValidate()
        {
            if (specials.Length > MaxSpecials)
            {
                Array.Resize(ref specials, MaxSpecials);
            }
        }

        // Resolved from the sibling Spine component rather than stored here,
        // so it can never drift out of sync with the prefab's actual rig.
        public SkeletonDataAsset SkeletonData => GetComponent<SkeletonRenderer>()?.skeletonDataAsset;

        // Level scaling is dropped for now (no growth curve) — returns the
        // flat base stats regardless of level. Always a fresh block so callers
        // can attach modifiers without affecting the shared asset.
        public StatBlock GetStatsAtLevel(int level)
        {
            return baseStats.CreateStats();
        }

        public bool TryGetBasicAttack(AttackInput input, out BasicAttackEntry entry)
        {
            foreach (var basicAttack in basicAttacks)
            {
                if (basicAttack.input == input)
                {
                    entry = basicAttack;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public bool TryGetSpecial(int slotIndex, out SpecialAttackEntry entry)
        {
            if (slotIndex >= 0 && slotIndex < specials.Length)
            {
                entry = specials[slotIndex];
                return true;
            }

            entry = default;
            return false;
        }

        public IEnumerable<CharacterState> EnumerateCharacterStateReferences()
        {
            if (defaultState != null)
            {
                yield return defaultState;
            }

            foreach (var basicAttack in basicAttacks)
            {
                if (basicAttack.state != null)
                {
                    yield return basicAttack.state;
                }
            }

            foreach (var combo in combos)
            {
                if (combo.finisherState != null)
                {
                    yield return combo.finisherState;
                }
            }

            foreach (var special in specials)
            {
                if (special.state != null)
                {
                    yield return special.state;
                }
            }
        }
    }
}
