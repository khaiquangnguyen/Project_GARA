using System;
using GARA.Input;

namespace GARA.InputCombos
{
    /// <summary>
    /// One ordered step of an <see cref="InputComboDefinition"/>. timeAllowed only matters
    /// in <see cref="ComboTimingMode.PerStepInterval"/> mode.
    /// </summary>
    [Serializable]
    public struct ComboStepDefinition
    {
        public InputToken input;
        public float scoreValue;
        public float timeAllowed;
        public ComboStepPayload payload;
    }
}
