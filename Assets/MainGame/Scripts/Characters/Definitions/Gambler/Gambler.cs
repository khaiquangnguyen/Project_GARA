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

        [Tooltip("The timing prompt every card plays. A single-roll card cheats its game on a hit; a per-press card plays one press per hit.")]
        [SerializeField]
        private Qte qte = Qte.Default;

        [Tooltip("Seconds the roll is shown before the card's move plays.")]
        [Min(0f)]
        [SerializeField]
        private float outcomeRevealSeconds = 1.2f;

        public SkillPerformanceTiering SpecialTiering => specialTiering;

        public float OutcomeRevealSeconds => outcomeRevealSeconds;

        public Qte Qte => qte;

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
