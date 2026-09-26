using GARA.Characters;
using GARA.InputSets;

namespace GARA.Characters.Chef
{
    // Turns an InputSetCompletionReport into a cooking-domain DishReport.
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

        // Report may cover the whole run or a single step.
        public static DishReport Evaluate(InputSetCompletionReport report, ChefSkillCard card, SkillPerformanceTier tier)
        {
            var setResults = report?.SetResults;
            var stepCount = setResults?.Count ?? 0;
            var steps = new StepReport[stepCount];
            var exquisiteSteps = 0;
            var ruinedSteps = 0;
            var anyBurnt = false;
            var hasKeyStep = false;
            StepReport keyStep = default;

            for (var i = 0; i < stepCount; i++)
            {
                var result = setResults[i];
                var quality = QualityOf(in result);
                var burnt = result.LastFailure == InputSetFailureReason.SetTimedOut;

                steps[i] = new StepReport(result.SetIndex, quality, burnt, result.Attempts, result.TimeToClear);

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

                if (result.SetIndex == card.KeyStepIndex)
                {
                    hasKeyStep = true;
                    keyStep = steps[i];
                }
            }

            return new DishReport(GradeOf(tier), steps, keyStep, hasKeyStep, exquisiteSteps, ruinedSteps, anyBurnt, report, card.Flavors);
        }
    }
}
