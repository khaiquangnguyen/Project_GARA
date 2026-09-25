using System;
using GARA.Characters;
using GARA.Rhythm;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    /// <summary>One bar of a rhythm card: its notes and the attack played live when all of them are hit.</summary>
    [Serializable]
    public struct RhythmCardBar
    {
        [Tooltip("Note times are absolute (seconds after the lead-in), not relative to their bar.")]
        public RhythmNote[] notes;

        [Tooltip("Played the moment the bar is cleared. Empty = nothing plays for this bar.")]
        [Expandable]
        public AttackAnimationSpec move;
    }
}
