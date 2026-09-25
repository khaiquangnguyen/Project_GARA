using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters
{
    // Each mode is a pool (enemies, friendlies, or every character) plus a
    // shape: One target, All of the pool, or Multi — a set number of picks
    // (see SkillCardDefinition.multiTargetCount). WithRepeat lets the same
    // character be picked more than once — each pick is its own hit, so
    // repeats stack (e.g. three arrows, two into the same enemy). NoRepeat
    // needs distinct characters, so it's capped at however many are alive.
    //
    // New values go at the end — this is serialized by int on authored assets.
    public enum SpecialTargetMode
    {
        OneEnemy,
        AllEnemy,
        OneFriendly,
        AllFriendly,
        MultiEnemyWithRepeat,
        MultiEnemyNoRepeat,
        MultiFriendlyWithRepeat,
        MultiFriendlyNoRepeat,
        OneCharacter,
        AllCharacters,
        MultiCharacterWithRepeat,
        MultiCharacterNoRepeat
    }

    public enum TargetPool
    {
        Enemies,
        Friendlies,
        Everyone
    }

    public static class SpecialTargetModeExtensions
    {
        public static TargetPool GetPool(this SpecialTargetMode mode)
        {
            switch (mode)
            {
                case SpecialTargetMode.OneFriendly:
                case SpecialTargetMode.AllFriendly:
                case SpecialTargetMode.MultiFriendlyWithRepeat:
                case SpecialTargetMode.MultiFriendlyNoRepeat:
                    return TargetPool.Friendlies;
                case SpecialTargetMode.OneCharacter:
                case SpecialTargetMode.AllCharacters:
                case SpecialTargetMode.MultiCharacterWithRepeat:
                case SpecialTargetMode.MultiCharacterNoRepeat:
                    return TargetPool.Everyone;
                default:
                    return TargetPool.Enemies;
            }
        }

        public static bool IsSingle(this SpecialTargetMode mode)
        {
            return mode is SpecialTargetMode.OneEnemy or SpecialTargetMode.OneFriendly or SpecialTargetMode.OneCharacter;
        }

        public static bool IsAll(this SpecialTargetMode mode)
        {
            return mode is SpecialTargetMode.AllEnemy or SpecialTargetMode.AllFriendly or SpecialTargetMode.AllCharacters;
        }

        public static bool IsMulti(this SpecialTargetMode mode)
        {
            return !mode.IsSingle() && !mode.IsAll();
        }

        public static bool AllowsRepeatTargets(this SpecialTargetMode mode)
        {
            return mode is SpecialTargetMode.MultiEnemyWithRepeat
                or SpecialTargetMode.MultiFriendlyWithRepeat
                or SpecialTargetMode.MultiCharacterWithRepeat;
        }

        // How many picks a Multi mode actually needs out of a pool of
        // poolSize living characters — NoRepeat can't ask for more than exist.
        public static int ResolveMultiTargetCount(this SpecialTargetMode mode, int multiTargetCount, int poolSize)
        {
            if (poolSize <= 0)
            {
                return 0;
            }

            var count = Mathf.Max(1, multiTargetCount);
            return mode.AllowsRepeatTargets() ? count : Mathf.Min(count, poolSize);
        }

        // Resolves targets out of an already-gathered pool of living
        // characters with no player choosing (enemy turns, passives): One
        // picks at random, All takes the whole pool, Multi picks at random
        // per its repeat rule. Empty when the pool is.
        public static List<T> PickRandomTargets<T>(this SpecialTargetMode mode, IReadOnlyList<T> pool, int multiTargetCount)
        {
            if (pool.Count == 0)
            {
                return new List<T>();
            }

            if (mode.IsAll())
            {
                return new List<T>(pool);
            }

            var count = mode.IsSingle() ? 1 : mode.ResolveMultiTargetCount(multiTargetCount, pool.Count);
            var picks = new List<T>(count);
            if (mode.AllowsRepeatTargets())
            {
                for (var i = 0; i < count; i++)
                {
                    picks.Add(pool[Random.Range(0, pool.Count)]);
                }

                return picks;
            }

            var remaining = new List<T>(pool);
            for (var i = 0; i < count; i++)
            {
                var index = Random.Range(0, remaining.Count);
                picks.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            return picks;
        }
    }
}
