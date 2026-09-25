using GARA.Characters;
using GARA.ShakeBalance;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.ShakeBalance
{
    // Class-agnostic skill card whose input minigame is a ShakeBalance run
    // (keep a drifting value centered with left/right, cash out whenever).
    // Lives outside GARA.Characters so that assembly never has to depend on
    // GARA.ShakeBalance directly.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Shake Balance Skill Card", fileName = "ShakeBalanceSkillCard")]
    public class ShakeBalanceSkillCard : SkillCardDefinition
    {
        [SerializeField]
        [Expandable]
        private ShakeBalanceDefinition balance;

        // Maps the run's 0-1 result to the 0-1 score. The result only
        // approaches 1, so a curve that reaches 1 early (e.g. at 0.9) makes
        // Perfect reachable.
        [SerializeField]
        private AnimationCurve scoreShaping = AnimationCurve.Linear(0, 0, 1, 1);

        public ShakeBalanceDefinition Balance => balance;

        public AnimationCurve ScoreShaping => scoreShaping;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new ShakeBalanceSkillInputSession(this, host.GetDriver<ShakeBalancePlayer>());
        }

        protected internal virtual SkillPerformance BuildPerformance(ShakeBalanceReport report)
        {
            return ShakeBalancePerformanceMapper.Map(report, this);
        }

        protected virtual void OnValidate()
        {
            if (balance == null)
            {
                Debug.LogWarning($"{name}: ShakeBalanceSkillCard has no balance definition assigned.", this);
                return;
            }

            if (!balance.UseCashOutToken && balance.Duration <= 0f)
            {
                Debug.LogWarning($"{name}: ShakeBalanceSkillCard's balance has no cash out token and no duration — it only ends when the player falls.", this);
            }
        }
    }
}
