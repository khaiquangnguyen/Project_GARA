using GARA.Characters;
using GARA.InputSets;

namespace GARA.Characters.Chef
{
    // Cooking-domain SkillPerformance.Details payload a RecipeSkillCard
    // attaches on top of the generic InputSet mapping, so Chef-specific
    // effects (DishStatModifierSkillEffect, KeyStepDamageSkillEffect) can
    // read back per-step quality without depending on raw InputSetResult.
    public sealed class DishReport
    {
        public readonly DishGrade Grade;
        public readonly StepReport[] Steps;
        public readonly StepReport KeyStep;
        public readonly bool HasKeyStep;
        public readonly int ExquisiteSteps;
        public readonly int RuinedSteps;
        public readonly bool AnyBurnt;
        public readonly InputSetCompletionReport Source;
        public readonly FlavorTag Flavors;

        public DishReport(
            DishGrade grade,
            StepReport[] steps,
            StepReport keyStep,
            bool hasKeyStep,
            int exquisiteSteps,
            int ruinedSteps,
            bool anyBurnt,
            InputSetCompletionReport source,
            FlavorTag flavors)
        {
            Grade = grade;
            Steps = steps;
            KeyStep = keyStep;
            HasKeyStep = hasKeyStep;
            ExquisiteSteps = exquisiteSteps;
            RuinedSteps = ruinedSteps;
            AnyBurnt = anyBurnt;
            Source = source;
            Flavors = flavors;
        }
    }
}
