using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.FrogBard
{
    // Holds what every Frog Bard card shares.
    public class FrogBard : CharacterDefinition
    {
        [Header("Specials (shared by every Frog Bard card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        [Tooltip("Lead-in, tail-out and judging windows around every Frog Bard card's bars.")]
        [SerializeField]
        private RhythmSequenceTiming specialTiming = RhythmSequenceTiming.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;
        public RhythmSequenceTiming SpecialTiming => specialTiming;

        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is FrogBardSkillCard))
                {
                    Debug.LogWarning($"{name}: Frog Bard skill cards must be FrogBardSkillCard. '{card.name}' is not.", this);
                }
            }
        }
    }
}
