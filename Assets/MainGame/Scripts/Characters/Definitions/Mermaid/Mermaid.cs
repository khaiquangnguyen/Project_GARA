using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Holds what every Mermaid card shares.
    public class Mermaid : CharacterDefinition
    {
        [Header("Specials (shared by every Mermaid card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        [Tooltip("Lead-in, tail-out and judging windows around every Mermaid card's bars.")]
        [SerializeField]
        private RhythmSequenceTiming specialTiming = RhythmSequenceTiming.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;
        public RhythmSequenceTiming SpecialTiming => specialTiming;

        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is MermaidSkillCard))
                {
                    Debug.LogWarning($"{name}: Mermaid skill cards must be MermaidSkillCard (rhythm-only). '{card.name}' is not.", this);
                }
            }
        }
    }
}
