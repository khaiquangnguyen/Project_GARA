using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // A recipe; every performance carries a DishReport for the Chef effects.
    [CreateAssetMenu(menuName = "GARA/Characters/Chef/Recipe Card", fileName = "RecipeCard")]
    public class ChefSkillCard : LiveInputSetSkillCard
    {
        [Header("Cooking")]
        [Tooltip("The step KeyStepDamage rewards when it comes out exquisite. -1 = none.")]
        [SerializeField]
        private int keyStepIndex = -1;

        public int KeyStepIndex => keyStepIndex;

        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is Chef chef ? chef.SpecialTiering : SkillPerformanceTiering.Default;
        }

        protected override InputSetScoreModel ScoreModelFor(CharacterDefinition actor)
        {
            return actor is Chef chef ? chef.SpecialScoreModel : InputSetScoreModel.Default;
        }

        protected override InputSetRetryPolicy RetryPolicyFor(CharacterDefinition actor)
        {
            return actor is Chef chef ? chef.SpecialRetryPolicy : InputSetRetryPolicy.Default;
        }

        protected override SkillPerformance BuildPerformance(InputSetCompletionReport report, SkillPerformanceTiering tiering, InputSetScoreModel scoreModel)
        {
            var performance = base.BuildPerformance(report, tiering, scoreModel);
            var dish = DishEvaluator.Evaluate(report, this, performance.Tier);
            return new SkillPerformance(performance.Score, performance.Tier, performance.WasAborted, dish);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            keyStepIndex = Mathf.Clamp(keyStepIndex, -1, Steps.Count - 1);
        }
    }
}
