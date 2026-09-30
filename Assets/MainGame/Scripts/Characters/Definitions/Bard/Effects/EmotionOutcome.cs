using System;
using System.Collections.Generic;

namespace GARA.Characters.Bard
{
    // What one side (joy or sadness) of a DualEmotionEffect does.
    [Serializable]
    public abstract class EmotionOutcome
    {
        public abstract void Apply(in SkillEffectContext context);

        // Living members only; the whole party includes the Bard.
        // RandomEnemies picks randomCount different enemies.
        protected static IReadOnlyList<ICombatTarget> TargetsOf(in SkillEffectContext context, OutcomeTargets targets, int randomCount = 1)
        {
            switch (targets)
            {
                case OutcomeTargets.WholeParty:
                    return PassiveEffectRunner.ResolveTargets(context.Battle, context.Self, SpecialTargetMode.AllFriendly);
                case OutcomeTargets.WholeEnemyParty:
                    return PassiveEffectRunner.ResolveTargets(context.Battle, context.Self, SpecialTargetMode.AllEnemy);
                case OutcomeTargets.RandomEnemies:
                    return PassiveEffectRunner.ResolveTargets(context.Battle, context.Self, SpecialTargetMode.MultiEnemyNoRepeat, randomCount);
                default:
                    return context.Targets;
            }
        }
    }
}
