using System;
using System.Collections.Generic;
using NaughtyAttributes;
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

        [Header("Skill Cards")]
        [Tooltip("Up to 4 entries — array index 0-3 maps to A/S/D/F.")]
        [SerializeField]
        [Expandable]
        private SkillCardDefinition[] skillCards = Array.Empty<SkillCardDefinition>();

        public IReadOnlyList<SkillCardDefinition> SkillCards => skillCards;

        [Header("Passives")]
        [Tooltip("Always-on class passives. React to combat events; runtime state lives on the participant, not the asset.")]
        [SerializeField]
        [Expandable]
        private PassiveDefinition[] passives = Array.Empty<PassiveDefinition>();

        public IReadOnlyList<PassiveDefinition> Passives => passives;

        [Header("Palate (enemies)")]
        public PalateProfile palate;

        private const int MaxSkillCards = 4;

        private void OnValidate()
        {
            if (skillCards.Length > MaxSkillCards)
            {
                Array.Resize(ref skillCards, MaxSkillCards);
            }

            palate.SanitizeInPlace(this);
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

        public bool TryGetSkillCard(int slotIndex, out SkillCardDefinition card)
        {
            if (slotIndex >= 0 && slotIndex < skillCards.Length && skillCards[slotIndex] != null)
            {
                card = skillCards[slotIndex];
                return true;
            }

            card = null;
            return false;
        }

        public IEnumerable<CharacterState> EnumerateCharacterStateReferences()
        {
            if (defaultState != null)
            {
                yield return defaultState;
            }

            foreach (var card in skillCards)
            {
                if (card != null && card.animationState != null)
                {
                    yield return card.animationState;
                }
            }
        }
    }
}
