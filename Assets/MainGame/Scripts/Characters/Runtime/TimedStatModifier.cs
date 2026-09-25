using System;
using System.Collections.Generic;

namespace GARA.Characters
{
    // One active buff/debuff instance produced by a skill effect. Implements
    // IStatModifierSource (see StatModifier.cs) so it plugs directly into
    // StatBlock.Apply/WithModifiers — the same pipeline equipment/passives
    // use — rather than a parallel system current stat resolution ignores.
    // Turn countdown/expiry and the list of live instances are owned by
    // whoever implements ICombatTarget.ApplyTimedModifier (CombatParticipant,
    // GARA.Combat side); this type only knows how to describe itself as a
    // StatModifier.
    public class TimedStatModifier : IStatModifierSource
    {
        public StatKind stat;
        public float magnitude;
        public int remainingTurns;
        public string sourceId;

        public TimedStatModifier(StatKind stat, float magnitude, int remainingTurns, string sourceId)
        {
            this.stat = stat;
            this.magnitude = magnitude;
            this.remainingTurns = remainingTurns;
            this.sourceId = sourceId;
        }

        public IEnumerable<StatModifier> GetModifiers()
        {
            yield return CreateModifier();
        }

        private StatModifier CreateModifier()
        {
            switch (stat)
            {
                case StatKind.MaxHp:
                    return new StatModifier<MaxHpStat> { Mode = ModifierMode.Flat, Value = magnitude };

                case StatKind.MaxMp:
                    return new StatModifier<MaxMpStat> { Mode = ModifierMode.Flat, Value = magnitude };

                case StatKind.MaxAp:
                    return new StatModifier<MaxApStat> { Mode = ModifierMode.Flat, Value = magnitude };

                case StatKind.Attack:
                    return new StatModifier<AttackStat> { Mode = ModifierMode.Flat, Value = magnitude };

                case StatKind.Defense:
                    return new StatModifier<DefenseStat> { Mode = ModifierMode.Flat, Value = magnitude };

                case StatKind.Speed:
                    return new StatModifier<SpeedStat> { Mode = ModifierMode.Flat, Value = magnitude };

                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unhandled StatKind.");
            }
        }
    }
}
