namespace GARA.Characters.Dancer
{
    // Details payload of the synthesized SkillPerformance handed to Spotlight
    // Lover's encore effects — lets a bespoke effect read back how many
    // encores fired and what triggered them via SkillPerformance.TryGetDetails.
    public sealed class EncoreTriggerReport
    {
        public readonly int encoreIndexThisResolution;
        public readonly int totalEncores;
        public readonly int remainingNotes;
        public readonly int threshold;
        public readonly SkillCardDefinition sourceCard;

        public EncoreTriggerReport(int encoreIndexThisResolution, int totalEncores, int remainingNotes, int threshold, SkillCardDefinition sourceCard)
        {
            this.encoreIndexThisResolution = encoreIndexThisResolution;
            this.totalEncores = totalEncores;
            this.remainingNotes = remainingNotes;
            this.threshold = threshold;
            this.sourceCard = sourceCard;
        }
    }
}
