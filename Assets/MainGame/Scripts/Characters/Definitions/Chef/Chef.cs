using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Holds what every Chef card shares.
    public class Chef : CharacterDefinition
    {
        [Header("Specials (shared by every Chef card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        [SerializeField]
        private InputSetScoreModel specialScoreModel = InputSetScoreModel.Default;

        [SerializeField]
        private InputSetRetryPolicy specialRetryPolicy = InputSetRetryPolicy.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;
        public InputSetScoreModel SpecialScoreModel => specialScoreModel;
        public InputSetRetryPolicy SpecialRetryPolicy => specialRetryPolicy;

        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is ChefSkillCard))
                {
                    Debug.LogWarning($"{name}: Chef skill cards must be ChefSkillCard. '{card.name}' is not.", this);
                }
            }
        }
    }
}
