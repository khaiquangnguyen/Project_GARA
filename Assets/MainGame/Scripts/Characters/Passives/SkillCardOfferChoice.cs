namespace GARA.Characters
{
    // The owner's answer to a SkillCardOffer. replaced is null for a free
    // slot.
    public readonly struct SkillCardOfferChoice
    {
        public readonly bool accept;
        public readonly SkillCardDefinition replaced;

        private SkillCardOfferChoice(bool accept, SkillCardDefinition replaced)
        {
            this.accept = accept;
            this.replaced = replaced;
        }

        public static SkillCardOfferChoice Equip => new SkillCardOfferChoice(true, null);

        public static SkillCardOfferChoice Decline => new SkillCardOfferChoice(false, null);

        public static SkillCardOfferChoice Replace(SkillCardDefinition replaced)
        {
            return new SkillCardOfferChoice(true, replaced);
        }
    }
}
