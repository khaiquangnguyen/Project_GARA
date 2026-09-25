using System;

namespace GARA.Characters
{
    // Float counterpart to RangeableInt — see that type for the rationale.
    [Serializable]
    public struct RangeableFloat
    {
        public float value;
        public bool useRange;
        public float min;
        public float max;

        public readonly float Fixed => value;

        public float Resolve(ICombatTarget actor)
        {
            if (useRange && max > min && actor != null && actor.Passives != null && actor.Passives.TryGetRangeRoller(out var roller))
            {
                return Resolve(roller);
            }

            return value;
        }

        public float Resolve(in SkillEffectContext context)
        {
            return Resolve(context.Self);
        }

        public float Resolve(IValueRangeRoller roller)
        {
            return roller.RollRange(min, max);
        }

        public static implicit operator RangeableFloat(float fixedValue)
        {
            return new RangeableFloat { value = fixedValue };
        }
    }
}
