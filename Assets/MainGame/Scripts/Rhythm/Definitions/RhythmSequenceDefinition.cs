using System;
using System.Collections.Generic;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>Runtime-built sequence of notes, grouped into bars, judged against an absolute timeline. Authored as a card's bars (see RhythmSkillCard).</summary>
    public class RhythmSequenceDefinition : ScriptableObject
    {
        private float _leadIn;
        private float _tailOut;
        private RhythmTimingWindows _windows;
        private RhythmBar[] _bars = Array.Empty<RhythmBar>();
        private RhythmNote[] _flatNotes;
        private int[] _barOfNote;

        public float LeadIn => _leadIn;
        public float TailOut => _tailOut;
        public RhythmTimingWindows Windows => _windows;
        public IReadOnlyList<RhythmBar> Bars => _bars;

        /// <summary>Every bar's notes flattened and sorted by time — what the runner judges.</summary>
        public IReadOnlyList<RhythmNote> Notes
        {
            get
            {
                EnsureFlattened();
                return _flatNotes;
            }
        }

        /// <summary>Which bar the note at this index of <see cref="Notes"/> belongs to.</summary>
        public int BarOfNote(int noteIndex)
        {
            EnsureFlattened();
            return _barOfNote[noteIndex];
        }

        /// <summary>Builds an in-memory sequence (not saved as an asset), one note per bar. Caller owns destroying it.</summary>
        public static RhythmSequenceDefinition CreateRuntime(float leadIn, float tailOut, RhythmTimingWindows windows, RhythmNote[] notes)
        {
            var definition = CreateInstance<RhythmSequenceDefinition>();
            definition._leadIn = leadIn;
            definition._tailOut = tailOut;
            definition._windows = windows;
            definition._bars = Array.ConvertAll(notes, note => new RhythmBar { notes = new[] { note } });
            return definition;
        }

        /// <summary>Builds an in-memory sequence (not saved as an asset) from timing and bars. Caller owns destroying it.</summary>
        public static RhythmSequenceDefinition CreateRuntime(RhythmSequenceTiming timing, RhythmBar[] bars)
        {
            var definition = CreateInstance<RhythmSequenceDefinition>();
            definition._leadIn = timing.leadIn;
            definition._tailOut = timing.tailOut;
            definition._windows = timing.windows;
            definition._bars = bars ?? Array.Empty<RhythmBar>();
            return definition;
        }

        /// <summary>Total seconds the sequence runs for, from Start() to the final Completed event.</summary>
        public float Duration
        {
            get
            {
                var maxNoteEnd = 0f;
                foreach (var note in Notes)
                {
                    var noteEnd = note.time + note.holdDuration;
                    if (noteEnd > maxNoteEnd)
                    {
                        maxNoteEnd = noteEnd;
                    }
                }

                return _leadIn + maxNoteEnd + _windows.ok + _tailOut;
            }
        }

        private void EnsureFlattened()
        {
            if (_flatNotes != null)
            {
                return;
            }

            var entries = new List<(RhythmNote note, int bar)>();
            for (var b = 0; b < _bars.Length; b++)
            {
                foreach (var note in _bars[b].notes ?? Array.Empty<RhythmNote>())
                {
                    entries.Add((note, b));
                }
            }

            entries.Sort((x, y) => x.note.time.CompareTo(y.note.time));
            _flatNotes = entries.ConvertAll(e => e.note).ToArray();
            _barOfNote = entries.ConvertAll(e => e.bar).ToArray();
        }
    }
}
