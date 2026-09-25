using System;

namespace GARA.Rhythm
{
    /// <summary>A group of any number of notes that maps, as a whole, to one move (see GARA.SkillCards.Rhythm.RhythmCardBar).</summary>
    [Serializable]
    public struct RhythmBar
    {
        public RhythmNote[] notes;
    }
}
