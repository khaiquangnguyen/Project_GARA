using System.Collections.Generic;

namespace GARA.Characters
{
    // One live beat of a skill card's input session: an attack for the card's
    // own state to play now (null = its own spec), and the effects its
    // impact resolves.
    public readonly struct SkillStep
    {
        public readonly AttackAnimationSpec animation;
        public readonly SkillPerformance performance;
        public readonly IReadOnlyList<ISkillEffect> effects;
        public readonly bool isFinale;

        public SkillStep(AttackAnimationSpec animation, SkillPerformance performance, IReadOnlyList<ISkillEffect> effects, bool isFinale)
        {
            this.animation = animation;
            this.performance = performance;
            this.effects = effects;
            this.isFinale = isFinale;
        }
    }
}
