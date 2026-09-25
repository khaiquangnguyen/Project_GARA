using System.Collections.Generic;
using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Chef's recipe minigame: an InputSetSkillCard whose sets are each
    // labeled with a RecipeStep, so its BuildPerformance can attach a
    // cooking-domain DishReport as the SkillPerformance.Details payload on
    // top of the generic InputSet score/tier mapping.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Chef/Recipe Skill Card", fileName = "RecipeSkillCard")]
    public class RecipeSkillCard : InputSetSkillCard
    {
        [SerializeField]
        private RecipeStep[] steps;

        [SerializeField]
        private int keyStepIndex = -1;

        [Header("Cooking")]
        [SerializeField]
        private FlavorTag flavors;

        [Tooltip("Set on a variation asset to point at the dish it is a re-seasoning of. UI groups variations under their base card; mechanically ignored.")]
        [SerializeField]
        private RecipeSkillCard variationOf;

        public IReadOnlyList<RecipeStep> Steps => steps;

        public int KeyStepIndex => keyStepIndex;

        public FlavorTag Flavors => flavors;

        public RecipeSkillCard VariationOf => variationOf;

        public bool IsVariation => variationOf != null;

        public RecipeSkillCard RootCard => variationOf != null ? variationOf : this;

        protected override SkillPerformance BuildPerformance(InputSetCompletionReport report)
        {
            var performance = base.BuildPerformance(report);
            var dish = DishEvaluator.Evaluate(report, this, performance.Tier);
            return new SkillPerformance(performance.Score, performance.Tier, performance.WasAborted, dish);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (SetCollection != null && steps != null && steps.Length != SetCollection.Sets.Length)
            {
                Debug.LogWarning($"{name}: steps.Length ({steps.Length}) does not match setCollection.sets.Length ({SetCollection.Sets.Length}).", this);
            }

            keyStepIndex = steps != null ? Mathf.Clamp(keyStepIndex, -1, steps.Length - 1) : -1;

            if (variationOf != null && variationOf.flavors == flavors)
            {
                Debug.LogWarning($"{name}: variationOf is set to {variationOf.name}, but both share the same flavors ({flavors}) — this isn't actually a re-seasoning.", this);
            }
        }
    }
}
