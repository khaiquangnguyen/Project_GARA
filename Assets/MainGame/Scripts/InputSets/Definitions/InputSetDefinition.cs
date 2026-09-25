using System;
using GARA.Input;

namespace GARA.InputSets
{
    /// <summary>
    /// One ordered list of inputs the player must press in order to clear this set.
    /// </summary>
    [Serializable]
    public struct InputSetDefinition
    {
        public InputToken[] inputs;
        public float timeLimit;
    }
}
