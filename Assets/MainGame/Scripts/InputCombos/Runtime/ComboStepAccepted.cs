namespace GARA.InputCombos
{
    /// <summary>
    /// Raised for every accepted combo step, including the final one that completes the combo.
    /// </summary>
    public readonly struct ComboStepAccepted
    {
        public int StepIndex { get; }
        public ComboStepDefinition Step { get; }
        public float TimeSinceComboStart { get; }
        public float TimeSincePreviousStep { get; }
        public float ScoreAfter { get; }

        public ComboStepAccepted(int stepIndex, ComboStepDefinition step, float timeSinceComboStart, float timeSincePreviousStep, float scoreAfter)
        {
            StepIndex = stepIndex;
            Step = step;
            TimeSinceComboStart = timeSinceComboStart;
            TimeSincePreviousStep = timeSincePreviousStep;
            ScoreAfter = scoreAfter;
        }
    }
}
