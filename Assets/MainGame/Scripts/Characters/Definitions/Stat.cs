using System.Collections.Generic;

namespace GARA.Characters
{
    // One stat of one character. Each concrete subclass is its own type, so
    // consumers and modifiers reference a stat by type rather than by tag.
    // A stat owns the modifiers targeting it and computes its own value.
    public abstract class Stat
    {
        private readonly List<StatModifier> _modifiers = new();

        protected Stat(int baseValue)
        {
            BaseValue = baseValue;
        }

        public int BaseValue { get; }

        public abstract string DisplayName { get; }

        // Percent modifiers are always taken against BaseValue, so the result
        // does not depend on the order sources were applied in.
        public int Value
        {
            get
            {
                float total = BaseValue;

                foreach (var modifier in _modifiers)
                {
                    total += modifier.Mode == ModifierMode.Flat
                        ? modifier.Value
                        : BaseValue * modifier.Value;
                }

                return (int)total;
            }
        }

        public IReadOnlyList<StatModifier> Modifiers => _modifiers;

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
        }

        public void RemoveModifier(StatModifier modifier)
        {
            _modifiers.Remove(modifier);
        }

        public void ClearModifiers()
        {
            _modifiers.Clear();
        }
    }
}
