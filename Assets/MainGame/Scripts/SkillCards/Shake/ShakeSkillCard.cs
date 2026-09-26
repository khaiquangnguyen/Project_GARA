using GARA.Characters;
using GARA.Shake;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Shake
{
    // Class-agnostic skill card whose input minigame is a ShakeInput
    // alternating-press session. Lives outside GARA.Characters so that
    // assembly never has to depend on GARA.Shake directly.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Shake Skill Card", fileName = "ShakeSkillCard")]
    public class ShakeSkillCard : TieredSkillCard
    {
        [SerializeField]
        [Expandable]
        private ShakeInputDefinition shakeInput;

        [SerializeField, Min(1)]
        private int targetPairs = 10;

        [SerializeField]
        private bool stopWhenTargetReached = true;

        [SerializeField, Min(0f)]
        private float wrongPressPenalty = 0f;

        [SerializeField]
        private AnimationCurve scoreShaping = AnimationCurve.Linear(0, 0, 1, 1);

        public ShakeInputDefinition ShakeInput => shakeInput;
        public int TargetPairs => targetPairs;
        public bool StopWhenTargetReached => stopWhenTargetReached;
        public float WrongPressPenalty => wrongPressPenalty;
        public AnimationCurve ScoreShaping => scoreShaping;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new ShakeSkillInputSession(this, host.GetDriver<ShakeInputPlayer>());
        }

        protected virtual void OnValidate()
        {
            if (shakeInput == null)
            {
                Debug.LogWarning($"[{name}] ShakeSkillCard: shakeInput is not assigned.", this);
                return;
            }

            if (shakeInput.Duration <= 0f && !stopWhenTargetReached)
            {
                Debug.LogWarning(
                    $"[{name}] ShakeSkillCard: shakeInput has no time cap and stopWhenTargetReached is false — this session would never end.",
                    this);
            }
        }
    }
}
