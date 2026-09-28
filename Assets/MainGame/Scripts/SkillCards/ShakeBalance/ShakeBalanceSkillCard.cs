using GARA.Characters;
using GARA.ShakeBalance;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.ShakeBalance
{
    // Class-agnostic skill card whose input minigame is a ShakeBalance run
    // (keep a drifting value centered with left/right to fill a meter).
    // Lives outside GARA.Characters so that assembly never has to depend on
    // GARA.ShakeBalance directly.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Shake Balance Skill Card", fileName = "ShakeBalanceSkillCard")]
    public class ShakeBalanceSkillCard : TieredSkillCard
    {
        [SerializeField]
        [Expandable]
        private ShakeBalanceDefinition balance;

        // Maps the run's result (1 filled, 0 failed) to the 0-1 score.
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
            }
        }
    }
}
