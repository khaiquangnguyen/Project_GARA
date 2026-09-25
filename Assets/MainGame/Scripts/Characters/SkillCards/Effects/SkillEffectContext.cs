using System.Collections.Generic;

namespace GARA.Characters
{
    // Payload handed to SkillEffectDefinition.Resolve. Mirrors
    // CharacterStateContext's shape plus the SkillPerformance the input
    // minigame produced.
    public readonly struct SkillEffectContext
    {
        public readonly IBattleQuery Battle;
        public readonly ICombatTarget Self;
        public readonly IReadOnlyList<ICombatTarget> Targets;
        public readonly SkillPerformance Performance;

        // Fraction of the card's full effect this resolve delivers (e.g. one
        // bar of a live rhythm card). 1 for a card that resolves once.
        public readonly float Share;

        public SkillEffectContext(IBattleQuery battle, ICombatTarget self, IReadOnlyList<ICombatTarget> targets, SkillPerformance performance, float share = 1f)
        {
            Battle = battle;
            Self = self;
            Targets = targets;
            Performance = performance;
            Share = share;
        }
    }
}
