namespace GARA.Characters.Filmmaker
{
    // One opponent skill being recorded; form is the last character seen
    // playing it.
    public sealed class SkillRecording
    {
        public readonly SkillCardDefinition card;
        public CharacterDefinition form;
        public int stacks;

        public SkillRecording(SkillCardDefinition card)
        {
            this.card = card;
        }
    }
}
