using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Dancer's own character definition. Subclassing CharacterDefinition
    // (rather than using it directly) gives this specific character a place
    // to grow unique personality/trait data and behavior overrides later,
    // without needing a generic "personality" system on the base class.
    public class Dancer : CharacterDefinition
    {
        [Header("Specials (shared by every Dancer card)")]
        [SerializeField]
        private SkillPerformanceTiering specialTiering = SkillPerformanceTiering.Default;

        [Tooltip("Lead-in, tail-out and judging windows around every Dancer card's bars.")]
        [SerializeField]
        private RhythmSequenceTiming specialTiming = RhythmSequenceTiming.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;
        public RhythmSequenceTiming SpecialTiming => specialTiming;

        // CharacterDefinition already declares its own private OnValidate
        // (duplicate card check) - Unity's MonoBehaviour message dispatch
        // invokes each class level's own OnValidate independently, so this
        // one doesn't need to (and, being private in the base, can't) call
        // base.OnValidate().
        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is DancerSkillCard))
                {
                    Debug.LogWarning($"{name}: Dancer skill cards must be DancerSkillCard (rhythm-only). '{card.name}' is not.", this);
                }
            }
        }
    }
}
