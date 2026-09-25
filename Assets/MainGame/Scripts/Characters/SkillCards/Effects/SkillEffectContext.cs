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

        public SkillEffectContext(IBattleQuery battle, ICombatTarget self, IReadOnlyList<ICombatTarget> targets, SkillPerformance performance)
        {
            Battle = battle;
            Self = self;
            Targets = targets;
            Performance = performance;
        }
    }
}
