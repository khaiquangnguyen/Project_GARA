using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Persistent, player-owned character. Serialized via Easy Save 3 as a
    // plain object — references its definition by ID, never by direct
    // prefab/component reference, so save data survives asset moves/renames.
    [Serializable]
    public class ManagedCharacter
    {

        public string instanceId;

        public string definitionId;


        public int level = 1;

        public int currentXp;


        public List<string> learnedAbilityIds = new();

        public EquipmentLoadout equipment = new();

        public List<string> carriedConsumableItemIds = new();

        public ManagedCharacter(string instanceId, string definitionId)
        {
            this.instanceId = instanceId;
            this.definitionId = definitionId;
        }

        // Definition base stats with any modifier sources (equipment, passives)
        // layered on. No sources exist yet — the parameter is the seam the
        // equipment system will use.
        public StatBlock GetEffectiveStats(CharacterDefinition definition, IEnumerable<IStatModifierSource> modifierSources = null)
        {
            var stats = definition.GetStatsAtLevel(level);
            stats.Apply(modifierSources);
            return stats;
        }

        public IEnumerable<string> GetAvailableAbilityIds(CharacterDefinition definition)
        {
            foreach (var id in definition.innateAbilityIds)
            {
                yield return id;
            }

            foreach (var entry in definition.levelUnlockedAbilities)
            {
                if (entry.level <= level)
                {
                    yield return entry.abilityId;
                }
            }

            foreach (var id in learnedAbilityIds)
            {
                yield return id;
            }
        }
    }
}
