using System;
using System.Collections.Generic;
using System.Linq;

namespace GARA.Characters
{
    // Shared helper for passives that need to resolve their own authored
    // SkillEffectDefinitions outside of a card's normal resolution (e.g.
    // firing a bonus effect on a high-tier hit). Builds the same kind of
    // SkillEffectContext CombatSceneManager builds for a card, but with
    // a target pool the passive chooses independently of whatever the
    // triggering card targeted.
    public static class PassiveEffectRunner
    {
        public static void Resolve(SkillEffectDefinition[] effects, in PassiveContext context, SpecialTargetMode targetMode, SkillPerformance triggerPerformance)
        {
            if (effects == null || effects.Length == 0)
            {
                return;
            }

            var targets = ResolveTargets(context.Battle, context.Self, targetMode);
            var effectContext = new SkillEffectContext(context.Battle, context.Self, targets, triggerPerformance);
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.Resolve(effectContext);
                }
            }
        }

        // Mirrors CombatSceneManager.TryUseSkillCard's targeting rules:
        // OneEnemy/OneFriendly pick a single living target, All* hits every
        // living member of the relevant pool. IBattleQuery.Allies already
        // excludes self, so OneFriendly falls back to self when there's no
        // other living ally, and AllFriendly adds self back in on top of
        // Allies — matching how CombatSceneManager's own LivingAlliesOf
        // (drawn from the actor's whole party, self included) behaves.
        public static IReadOnlyList<ICombatTarget> ResolveTargets(IBattleQuery battle, ICombatTarget self, SpecialTargetMode mode)
        {
            switch (mode)
            {
                case SpecialTargetMode.OneEnemy:
                {
                    var enemy = battle.Enemies.FirstOrDefault(candidate => !candidate.IsDefeated);
                    return enemy != null ? new[] { enemy } : Array.Empty<ICombatTarget>();
                }
                case SpecialTargetMode.AllEnemy:
                {
                    return battle.Enemies.Where(candidate => !candidate.IsDefeated).ToArray();
                }
                case SpecialTargetMode.OneFriendly:
                {
                    var ally = battle.Allies.FirstOrDefault(candidate => !candidate.IsDefeated);
                    if (ally != null)
                    {
                        return new[] { ally };
                    }
                    return self != null && !self.IsDefeated ? new[] { self } : Array.Empty<ICombatTarget>();
                }
                case SpecialTargetMode.AllFriendly:
                {
                    var living = battle.Allies.Where(candidate => !candidate.IsDefeated).ToList();
                    if (self != null && !self.IsDefeated)
                    {
                        living.Add(self);
                    }
                    return living;
                }
                default:
                    return Array.Empty<ICombatTarget>();
            }
        }
    }
}
