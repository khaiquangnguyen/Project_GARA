using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Dancer's own character definition. Subclassing CharacterDefinition
    // (rather than using it directly) gives this specific character a place
    // to grow unique personality/trait data and behavior overrides later,
    // without needing a generic "personality" system on the base class.
    public class Dancer : CharacterDefinition
    {
        // CharacterDefinition already declares its own private OnValidate
        // (array-length clamping) - Unity's MonoBehaviour message dispatch
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
