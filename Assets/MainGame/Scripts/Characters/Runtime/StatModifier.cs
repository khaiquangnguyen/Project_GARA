using System;
using System.Collections.Generic;

namespace GARA.Characters
{
    public enum ModifierMode
    {
        Flat,
        PercentOfBase
    }

    // Non-generic base so heterogeneous modifiers can live in one list; the
    // stat it targets is identified by the concrete Stat subclass it names.
    public abstract class StatModifier
    {
        public ModifierMode Mode;
        public float Value;

        public abstract Type TargetStat { get; }
    }

    public class StatModifier<TStat> : StatModifier where TStat : Stat
    {
        public override Type TargetStat => typeof(TStat);
    }

    // Implemented by anything that can contribute stat changes to a character:
    // equipment, passive abilities, status effects. StatBlock only ever depends
    // on this interface, so new modifier sources plug in without any change to
    // the calculation pipeline.
    public interface IStatModifierSource
    {
        IEnumerable<StatModifier> GetModifiers();
    }
}
