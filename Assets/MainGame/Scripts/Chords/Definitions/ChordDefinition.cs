using System;
using GARA.Input;

namespace GARA.Chords
{
    /// <summary>
    /// A set of tokens that must all be held down together, within a simultaneity tolerance,
    /// order irrelevant.
    /// </summary>
    [Serializable]
    public struct ChordDefinition
    {
        public InputToken[] required;
        public float timeLimit;
        public float simultaneityWindow;
    }
}
