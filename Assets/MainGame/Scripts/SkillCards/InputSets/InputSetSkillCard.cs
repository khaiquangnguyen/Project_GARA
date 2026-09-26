using GARA.Characters;
using GARA.InputSets;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.InputSets
{
    // Class-agnostic skill card driven by the InputSets minigame engine
    // (press the right token, in order, within each set's time limit).
    // Any character can use this directly; a class-specific subclass (e.g.
    // Chef's RecipeSkillCard) can override BuildPerformance to attach a
    // bespoke Details payload without this class needing to know about it.
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

        // protected internal: InputSetSkillInputSession (same assembly)
        // calls this directly to build the SkillPerformance it hands back
        // through onCompleted; RecipeSkillCard (a subclass in a different
        // assembly) overrides it to attach a DishReport as Details.
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
