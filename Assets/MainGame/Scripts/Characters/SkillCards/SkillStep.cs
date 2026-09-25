namespace GARA.Characters
{
    // One live beat of a skill card's input session: an attack for the card's
    // own state to play now (null = its own spec), and the share of the
    // card's effects (or its perfect effects, for the finale) its impact
    // resolves.
    public readonly struct SkillStep
    {
        public readonly AttackAnimationSpec animation;
        public readonly SkillPerformance performance;
        public readonly float share;
        public readonly bool isFinale;

        public SkillStep(AttackAnimationSpec animation, SkillPerformance performance, float share, bool isFinale)
        {
            this.animation = animation;
            this.performance = performance;
            this.share = share;
            this.isFinale = isFinale;
        }
    }
}
