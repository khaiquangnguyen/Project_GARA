using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // Holds what every Bard card shares.
    public class Bard : CharacterDefinition
    {
        [Header("Specials (shared by every Bard card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        [Tooltip("Lead-in, tail-out and judging windows around every Bard card's bars.")]
        [SerializeField]
        private RhythmSequenceTiming specialTiming = RhythmSequenceTiming.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;
        public RhythmSequenceTiming SpecialTiming => specialTiming;

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
