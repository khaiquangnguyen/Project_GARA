using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Enemy
{
    // Enemy prefab root; its cards have no minigame, so the AI or a player
    // can play them alike.
    public class Enemy : CharacterDefinition
    {
        private void OnValidate()
        {
            foreach (var card in SkillCards)
            {
                if (card != null && !(card is EnemySkillCard))
                {
                    Debug.LogWarning($"{name}: Enemy skill cards must be EnemySkillCard. '{card.name}' is not.", this);
                }
            }
        }
    }
}
