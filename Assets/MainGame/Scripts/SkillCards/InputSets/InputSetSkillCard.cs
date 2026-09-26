using GARA.Characters;
using GARA.InputSets;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.InputSets
{
    // Class-agnostic, non-live skill card driven by an input-set run.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Input Set Skill Card", fileName = "InputSetSkillCard")]
    public class InputSetSkillCard : TieredSkillCard
    {
        [SerializeField]
        [Expandable]
        private InputSetCollectionDefinition setCollection;

        [SerializeField]
        private InputSetScoreModel scoreModel = InputSetScoreModel.Default;

        public InputSetCollectionDefinition SetCollection => setCollection;

        public InputSetScoreModel ScoreModel => scoreModel;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new InputSetSkillInputSession(this, host.GetDriver<InputSetCollectionPlayer>());
        }

        protected internal virtual SkillPerformance BuildPerformance(InputSetCompletionReport report)
        {
            return InputSetPerformanceMapper.Map(report, this);
        }

        protected virtual void OnValidate()
        {
            if (setCollection == null)
            {
                Debug.LogWarning($"{name}: InputSetSkillCard has no set collection assigned.", this);
            }
        }
    }
}
