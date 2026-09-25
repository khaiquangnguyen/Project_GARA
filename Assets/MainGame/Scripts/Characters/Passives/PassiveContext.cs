using System.Collections.Generic;

namespace GARA.Characters
{
    // Payload handed to a PassiveDefinition when it's notified of a combat
    // event. Mirrors SkillEffectContext's shape plus the card/targets that
    // triggered the notification. Card may be null for future non-card
    // triggers (e.g. a turn-start or on-hit event with no authored card).
    public readonly struct PassiveContext
    {
        public readonly IBattleQuery Battle;
        public readonly ICombatTarget Self;
        public readonly SkillCardDefinition Card;
        public readonly IReadOnlyList<ICombatTarget> CardTargets;
        public readonly SkillPerformance Performance;

        public PassiveContext(IBattleQuery battle, ICombatTarget self, SkillCardDefinition card, IReadOnlyList<ICombatTarget> cardTargets, SkillPerformance performance)
        {
            Battle = battle;
            Self = self;
            Card = card;
            CardTargets = cardTargets;
            Performance = performance;
        }
    }
}
