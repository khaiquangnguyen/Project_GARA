using System;
using GARA.Input;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>A single note on a rhythm sequence's absolute timeline.</summary>
    [Serializable]
    public struct RhythmNote
    {
        public InputToken input;
        public RhythmNoteKind kind;

        /// <summary>Seconds after the sequence's lead-in when this note should be pressed.</summary>
        public float time;

        /// <summary>Only meaningful when <see cref="kind"/> is <see cref="RhythmNoteKind.Hold"/>.</summary>
        public float holdDuration;
    }
}
