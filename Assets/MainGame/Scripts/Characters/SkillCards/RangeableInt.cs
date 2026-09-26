using System;

namespace GARA.Characters
{
    // A fixed int or a min/max range rolled through the actor's passives'
    // IValueRangeRoller.
    [Serializable]
    public struct RangeableInt
    {
        public int value;
        public bool useRange;
        public int min;
        public int max;

        public readonly int Fixed => value;

        public int Resolve(ICombatTarget actor)
        {
            if (useRange && max > min && actor != null && actor.Passives != null && actor.Passives.TryGetRangeRoller(out var roller))
            {
                return Resolve(roller);
            }

            return value;
        }

        public int Resolve(in SkillEffectContext context)
        {
            return Resolve(context.Self);
        }

        public int Resolve(IValueRangeRoller roller)
        {
            return roller.RollRange(min, max);
        }

        public static implicit operator RangeableInt(int fixedValue)
        {
            return new RangeableInt { value = fixedValue };
        }
    }
}
