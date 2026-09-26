using System;
using GARA.Characters;
using GARA.Rhythm;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    /// <summary>One bar of a rhythm card: its notes, the attack it plays, and the effects whose gates decide whether it plays.</summary>
    [Serializable]
    public struct RhythmCardBar
    {
        [Tooltip("Note times are absolute (seconds after the lead-in), not relative to their bar.")]
        public RhythmNote[] notes;

        [Tooltip("Played once the bar's notes are judged, if any effect's gate passes. Empty = nothing plays for this bar.")]
        [Expandable]
        public AttackAnimationSpec move;

        [Tooltip("Gated on this bar's notes; the triggered ones apply on the move's hit. Needs a move.")]
        [SerializeReference]
        [SubclassPicker]
        public RhythmStepEffect[] effects;
    }
}
