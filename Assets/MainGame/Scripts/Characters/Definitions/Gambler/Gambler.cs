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

        [Tooltip("Landing the exact count cheats the card's game.")]
        [SerializeField]
        private CheatShake cheatShake = CheatShake.Default;

        public SkillPerformanceTiering SpecialTiering => specialTiering;

        public CheatShake CheatShake => cheatShake;

        private void OnValidate()
        {
            if (cheatShake.leftToken == cheatShake.rightToken || cheatShake.doneToken == cheatShake.leftToken || cheatShake.doneToken == cheatShake.rightToken)
            {
                Debug.LogWarning($"{name}: the cheat shake's left, right and done tokens must all differ.", this);
            }

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
