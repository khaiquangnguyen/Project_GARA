using System;
using GARA.Input;

namespace GARA.Rhythm
{
    /// <summary>Which way a note for <see cref="token"/> points on screen.</summary>
    [Serializable]
    public struct RhythmTokenDirection
    {
        public InputToken token;
        public RhythmArrowDirection direction;
    }
}
