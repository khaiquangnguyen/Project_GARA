using GARA.Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Combat
{
    public enum PromotionPolicy
    {
        None,
        RecruitableOnDefeat,
        RivalPersistsAcrossEncounters,
        SpecialStoryUnit
    }

    // Encounter-scoped enemy spawn data. Direct CharacterDefinition
    // reference is fine here — this is design-time authored content, not
    // save data. Most enemies never become a ManagedCharacter; PromotionPolicy
    // is the seam for the ones that eventually will.
    [CreateAssetMenu(menuName = "GARA/Combat/Enemy Encounter Entry", fileName = "NewEnemyEncounter")]
    public class EnemyEncounterData : ScriptableObject
    {
        [FormerlySerializedAs("Definition")]
        public CharacterDefinition definition;
        [FormerlySerializedAs("Level")]
        public int level = 1;

        [FormerlySerializedAs("PromotionPolicy")]
        public PromotionPolicy promotionPolicy = PromotionPolicy.None;
    }
}
