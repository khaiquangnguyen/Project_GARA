using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Throws coins; the move strikes once per heads.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Coin Throw Card", fileName = "CoinThrowCard")]
    public class CoinThrowSkillCard : GamblerSkillCard
    {
        [Tooltip("Seconds a strike gets before the next one.")]
        [BoxGroup("Coin Throw")]
        [Min(0)]
        [SerializeField]
        private float secondsBetweenStrikes = 0.6f;

        public override float SecondsBetweenStrikes => secondsBetweenStrikes;

        public override int StrikesFor(GambleOutcome outcome)
        {
            return outcome is CoinTossOutcome coins ? coins.HeadsCount : 0;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (game != null && !(game is CoinTossGame))
            {
                Debug.LogWarning($"{name}: CoinThrowSkillCard needs a {nameof(CoinTossGame)}.", this);
            }
        }
    }
}
