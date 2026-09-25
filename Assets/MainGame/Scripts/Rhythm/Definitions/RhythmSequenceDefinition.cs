using System;
using System.Collections.Generic;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>Designer-authored sequence of notes, grouped into bars, judged against an absolute timeline.</summary>
    [CreateAssetMenu(menuName = "GARA/Rhythm/Rhythm Sequence", fileName = "RhythmSequence")]
    public class RhythmSequenceDefinition : ScriptableObject
    {
        [SerializeField]
        private float leadIn = 1f;

        [SerializeField]
        private float tailOut = 0.25f;

        [SerializeField]
        private RhythmTimingWindows windows;

        [Tooltip("Note times are absolute (seconds after the lead-in), not relative to their bar.")]
        [SerializeField]
        private RhythmBar[] bars = Array.Empty<RhythmBar>();

        [NonSerialized] private RhythmNote[] _flatNotes;
        [NonSerialized] private int[] _barOfNote;

        public float LeadIn => leadIn;
        public float TailOut => tailOut;
        public RhythmTimingWindows Windows => windows;
        public IReadOnlyList<RhythmBar> Bars => bars;

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
            definition.leadIn = leadIn;
            definition.tailOut = tailOut;
            definition.windows = windows;
            definition.bars = Array.ConvertAll(notes, note => new RhythmBar { notes = new[] { note } });
            return definition;
        }

        /// <summary>Builds an in-memory sequence (not saved as an asset) from timing and bars. Caller owns destroying it.</summary>
        public static RhythmSequenceDefinition CreateRuntime(RhythmSequenceTiming timing, RhythmBar[] bars)
        {
            var definition = CreateInstance<RhythmSequenceDefinition>();
            definition.leadIn = timing.leadIn;
            definition.tailOut = timing.tailOut;
            definition.windows = timing.windows;
            definition.bars = bars ?? Array.Empty<RhythmBar>();
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

                return leadIn + maxNoteEnd + windows.ok + tailOut;
            }
        }

        private void EnsureFlattened()
        {
            if (_flatNotes != null)
            {
                return;
            }

            var entries = new List<(RhythmNote note, int bar)>();
            for (var b = 0; b < bars.Length; b++)
            {
                foreach (var note in bars[b].notes ?? Array.Empty<RhythmNote>())
                {
                    entries.Add((note, b));
                }
            }

            entries.Sort((x, y) => x.note.time.CompareTo(y.note.time));
            _flatNotes = entries.ConvertAll(e => e.note).ToArray();
            _barOfNote = entries.ConvertAll(e => e.bar).ToArray();
        }

        private void OnValidate()
        {
            for (var b = 0; b < bars.Length; b++)
            {
                var notes = bars[b].notes ?? Array.Empty<RhythmNote>();
                for (var i = 0; i < notes.Length; i++)
                {
                    notes[i].time = Mathf.Max(0f, notes[i].time);
                    notes[i].holdDuration = Mathf.Max(0f, notes[i].holdDuration);
                }

                Array.Sort(notes, (x, y) => x.time.CompareTo(y.time));
                bars[b].notes = notes;
            }

            _flatNotes = null;
            _barOfNote = null;
            WarnOverlappingNotes();
        }

        private void WarnOverlappingNotes()
        {
            var notes = Notes;
            for (var i = 0; i < notes.Count; i++)
            {
                for (var j = i + 1; j < notes.Count; j++)
                {
                    if (!(notes[i].input == notes[j].input))
                    {
                        continue;
                    }

                    if (Mathf.Abs(notes[i].time - notes[j].time) < windows.ok * 2f)
                    {
                        Debug.LogWarning($"[{name}] RhythmSequenceDefinition: notes {i} and {j} share an input token and have overlapping timing windows.", this);
                    }
                }
            }
        }
    }
}
