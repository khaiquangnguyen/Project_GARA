using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Holds what every Gambler card shares.
    public class Gambler : CharacterDefinition
    {
        [Header("Specials (shared by every Gambler card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;

        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is GamblerSkillCard))
                {
                    Debug.LogWarning($"{name}: Gambler skill cards must be GamblerSkillCard. '{card.name}' is not.", this);
                }
            }
        }
    }
}
