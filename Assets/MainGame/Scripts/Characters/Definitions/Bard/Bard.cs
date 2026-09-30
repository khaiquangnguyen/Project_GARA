using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // Holds what every Bard card shares.
    public class Bard : CharacterDefinition
    {
        [Header("Specials (shared by every Bard card)")]
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
                if (card != null && !(card is BardSkillCard))
                {
                    Debug.LogWarning($"{name}: Bard skill cards must be BardSkillCard. '{card.name}' is not.", this);
                }
            }
        }
    }
}
