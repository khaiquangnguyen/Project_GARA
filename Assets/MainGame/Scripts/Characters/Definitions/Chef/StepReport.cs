namespace GARA.Characters.Chef
{
    // One step's InputSetResult in cooking terms; Index is the card's step.
    public readonly struct StepReport
    {
        public readonly int Index;
        public readonly StepQuality Quality;
        public readonly bool Burnt;
        public readonly int Attempts;
        public readonly float TimeToClear;

        public StepReport(int index, StepQuality quality, bool burnt, int attempts, float timeToClear)
        {
            Index = index;
            Quality = quality;
            Burnt = burnt;
            Attempts = attempts;
            TimeToClear = timeToClear;
        }
    }
}
