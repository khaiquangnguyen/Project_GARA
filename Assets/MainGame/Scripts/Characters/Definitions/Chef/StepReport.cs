namespace GARA.Characters.Chef
{
    // Result of one recipe step's underlying InputSetResult, reshaped into
    // cooking-domain terms.
    public readonly struct StepReport
    {
        public readonly int Index;
        public readonly RecipeStep Step;
        public readonly StepQuality Quality;
        public readonly bool Burnt;
        public readonly int Attempts;
        public readonly float TimeToClear;

        public StepReport(int index, RecipeStep step, StepQuality quality, bool burnt, int attempts, float timeToClear)
        {
            Index = index;
            Step = step;
            Quality = quality;
            Burnt = burnt;
            Attempts = attempts;
            TimeToClear = timeToClear;
        }
    }
}
