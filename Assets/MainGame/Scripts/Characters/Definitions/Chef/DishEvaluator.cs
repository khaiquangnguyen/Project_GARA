using System;
using GARA.Characters;
using GARA.InputSets;

namespace GARA.Characters.Chef
{
    // Turns a raw InputSetCompletionReport (plus the authored RecipeStep[]
    // and overall SkillPerformanceTier) into a cooking-domain DishReport.
    public static class DishEvaluator
    {
        public static StepQuality QualityOf(in InputSetResult result)
        {
            if (result.Outcome != InputSetOutcome.Cleared)
            {
                return StepQuality.Ruined;
            }

            if (result.ClearedFirstTry)
            {
                return StepQuality.Exquisite;
            }

            if (result.Attempts <= 2)
            {
                return StepQuality.Fine;
            }

            return StepQuality.Rough;
        }

        public static DishGrade GradeOf(SkillPerformanceTier tier)
        {
            switch (tier)
            {
                case SkillPerformanceTier.Perfect:
                    return DishGrade.Exquisite;

                case SkillPerformanceTier.Good:
                    return DishGrade.Tasty;

                case SkillPerformanceTier.Ok:
                    return DishGrade.Edible;

                default:
                    return DishGrade.Ruined;
            }
        }

        public static DishReport Evaluate(InputSetCompletionReport report, RecipeSkillCard card, SkillPerformanceTier tier)
        {
            var setResults = report?.SetResults;
            var recipeSteps = card.Steps;

            var stepCount = 0;
            if (setResults != null && recipeSteps != null)
            {
                stepCount = Math.Min(setResults.Count, recipeSteps.Count);
            }

            var steps = new StepReport[stepCount];
            var exquisiteSteps = 0;
            var ruinedSteps = 0;
            var anyBurnt = false;

            for (var i = 0; i < stepCount; i++)
            {
                var result = setResults[i];
                var quality = QualityOf(in result);
                var burnt = result.LastFailure == InputSetFailureReason.SetTimedOut;

                steps[i] = new StepReport(i, recipeSteps[i], quality, burnt, result.Attempts, result.TimeToClear);

                if (quality == StepQuality.Exquisite)
                {
                    exquisiteSteps++;
                }

                if (quality == StepQuality.Ruined)
                {
                    ruinedSteps++;
                }

                if (burnt)
                {
                    anyBurnt = true;
                }
            }

            var keyStepIndex = card.KeyStepIndex;
            var hasKeyStep = keyStepIndex >= 0 && keyStepIndex < steps.Length;
            var keyStep = hasKeyStep ? steps[keyStepIndex] : default;

            var grade = GradeOf(tier);

            return new DishReport(grade, steps, keyStep, hasKeyStep, exquisiteSteps, ruinedSteps, anyBurnt, report, card.Flavors);
        }
    }
}
