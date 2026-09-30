using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Mimic
{
    // Has no specials of its own: its hand is its opponents' skills (see
    // AspiringActorPassive), each played in the form of an opponent that has it.
    public class Mimic : CharacterDefinition
    {
        private void OnValidate()
        {
            if (SkillCards.Count > 0)
            {
                Debug.LogWarning($"{name}: the Mimic's hand comes from its opponents — its own specials are added alongside the copies.", this);
            }
        }
    }
}
