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
        [Tooltip("How many skill cards this character can bring into a single battle.")]
        [Min(1)]
        public int maxCombatSkillCards = 4;

        // Every card this character can know: the card of each skill-card
        // state under it, in hierarchy order. The full pool, not what's usable
        // in a battle — combat reads the participant's resolved loadout.
        public IReadOnlyList<SkillCardDefinition> SkillCards
        {
            get
            {
                var cards = new List<SkillCardDefinition>();
                foreach (var state in GetComponentsInChildren<CharacterState>(true))
                {
                    if (state.SkillCard != null)
                    {
                        cards.Add(state.SkillCard);
                    }
                }

                return cards;
            }
        }

        [Header("Passives")]
        [Tooltip("Always-on class passives. React to combat events; runtime state lives on the participant, not the asset.")]
        [SerializeField]
        [Expandable]
        private PassiveDefinition[] passives = Array.Empty<PassiveDefinition>();

        public IReadOnlyList<PassiveDefinition> Passives => passives;

        [Header("Palate (enemies)")]
        public PalateProfile palate;

        [Header("HP Heart (enemies)")]
        [Tooltip("Where this enemy's HP heart sits.")]
        public Transform hpHeartAnchor;

        private void OnValidate()
        {
            palate.SanitizeInPlace(this);

            var seen = new HashSet<SkillCardDefinition>();
            foreach (var card in SkillCards)
            {
                if (!seen.Add(card))
                {
                    Debug.LogError($"{name}: more than one state references skill card '{card.name}' — only one may.", card);
                }
            }
        }

        // The state on this character that plays card, or null.
        public CharacterState FindSkillCardState(SkillCardDefinition card)
        {
            if (card == null)
            {
                return null;
            }

            foreach (var state in GetComponentsInChildren<CharacterState>(true))
            {
                if (state.SkillCard == card)
                {
                    return state;
                }
            }

            return null;
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

        // Picks the cards a character brings into a battle: the pool cards
        // whose cardId appears in equippedCardIds, in that order, capped at
        // maxCombatSkillCards. Unknown and duplicate ids are skipped. With
        // nothing equipped (e.g. enemies, which have no save data), falls
        // back to the first maxCombatSkillCards cards of the pool.
        public List<SkillCardDefinition> ResolveCombatLoadout(IReadOnlyList<string> equippedCardIds = null)
        {
            var pool = SkillCards;
            var loadout = new List<SkillCardDefinition>();
            if (equippedCardIds != null && equippedCardIds.Count > 0)
            {
                foreach (var cardId in equippedCardIds)
                {
                    if (loadout.Count >= maxCombatSkillCards)
                    {
                        break;
                    }

                    SkillCardDefinition card = null;
                    foreach (var candidate in pool)
                    {
                        if (candidate.cardId == cardId)
                        {
                            card = candidate;
                            break;
                        }
                    }

                    if (card != null && !loadout.Contains(card))
                    {
                        loadout.Add(card);
                    }
                }

                return loadout;
            }

            foreach (var card in pool)
            {
                if (loadout.Count >= maxCombatSkillCards)
                {
                    break;
                }

                if (!loadout.Contains(card))
                {
                    loadout.Add(card);
                }
            }

            return loadout;
        }

        public IEnumerable<CharacterState> EnumerateCharacterStateReferences()
        {
            if (defaultState != null)
            {
                yield return defaultState;
            }

            foreach (var state in GetComponentsInChildren<CharacterState>(true))
            {
                if (state.SkillCard != null)
                {
                    yield return state;
                }
            }
        }
    }
}
