using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Filmmaker
{
    // Holds what every Filmmaker card shares.
    public class Filmmaker : CharacterDefinition
    {
        [Header("Specials (shared by every Filmmaker card)")]
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
                if (card != null && !(card is FilmmakerSkillCard))
                {
                    Debug.LogWarning($"{name}: Filmmaker skill cards must be FilmmakerSkillCard. '{card.name}' is not.", this);
                }
            }
        }
    }
}
