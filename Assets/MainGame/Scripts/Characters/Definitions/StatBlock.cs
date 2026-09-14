using System;
using System.Collections.Generic;

namespace GARA.Characters
{
    // One live instance of every stat a character has. Always mutable and
    // always owned by a single character/participant — never share one
    // instance between layers, use Snapshot to hand a flattened copy on.
    public class StatBlock
    {
        private readonly Dictionary<Type, Stat> _byType;

        public StatBlock(int maxHp, int maxMp, int maxAp, int attack, int defense, int speed)
        {
            MaxHp = new MaxHpStat(maxHp);
            MaxMp = new MaxMpStat(maxMp);
            MaxAp = new MaxApStat(maxAp);
            Attack = new AttackStat(attack);
            Defense = new DefenseStat(defense);
            Speed = new SpeedStat(speed);

            _byType = new Dictionary<Type, Stat>
            {
                { typeof(MaxHpStat), MaxHp },
                { typeof(MaxMpStat), MaxMp },
                { typeof(MaxApStat), MaxAp },
                { typeof(AttackStat), Attack },
                { typeof(DefenseStat), Defense },
                { typeof(SpeedStat), Speed }
            };
        }

        public MaxHpStat MaxHp { get; }
        public MaxMpStat MaxMp { get; }
        public MaxApStat MaxAp { get; }
        public AttackStat Attack { get; }
        public DefenseStat Defense { get; }
        public SpeedStat Speed { get; }

        public IEnumerable<Stat> All => _byType.Values;

        public void Apply(StatModifier modifier)
        {
            if (_byType.TryGetValue(modifier.TargetStat, out var stat))
            {
                stat.AddModifier(modifier);
            }
        }

        public void Apply(IEnumerable<IStatModifierSource> sources)
        {
            if (sources == null)
            {
                return;
            }

            foreach (var source in sources)
            {
                foreach (var modifier in source.GetModifiers())
                {
                    Apply(modifier);
                }
            }
        }

        // Bakes every stat's current value into a fresh block's base values,
        // dropping the modifiers that produced them. This is how one layer's
        // result becomes the next layer's baseline.
        public StatBlock Snapshot()
        {
            return new StatBlock(MaxHp.Value, MaxMp.Value, MaxAp.Value, Attack.Value, Defense.Value, Speed.Value);
        }

        public StatBlock WithModifiers(IEnumerable<IStatModifierSource> sources)
        {
            var result = Snapshot();
            result.Apply(sources);
            return result;
        }
    }
}
