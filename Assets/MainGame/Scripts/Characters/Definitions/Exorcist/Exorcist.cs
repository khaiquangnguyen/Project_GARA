using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Holds what every Exorcist card shares.
    public class Exorcist : CharacterDefinition
    {
        [Header("Specials (shared by every Exorcist card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        [Tooltip("Lead-in, tail-out and judging windows around every Exorcist card's bars.")]
        [SerializeField]
        private RhythmSequenceTiming specialTiming = RhythmSequenceTiming.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;
        public RhythmSequenceTiming SpecialTiming => specialTiming;

        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is ExorcistSkillCard))
                {
                    Debug.LogWarning($"{name}: Exorcist skill cards must be ExorcistSkillCard (rhythm-only). '{card.name}' is not.", this);
                }
            }
        }
    }
}
