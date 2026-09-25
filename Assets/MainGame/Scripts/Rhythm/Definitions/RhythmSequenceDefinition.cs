using System;
using System.Collections.Generic;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>Designer-authored sequence of notes to be judged against an absolute timeline.</summary>
    [CreateAssetMenu(menuName = "GARA/Rhythm/Rhythm Sequence", fileName = "RhythmSequence")]
    public class RhythmSequenceDefinition : ScriptableObject
    {
        [SerializeField]
        private float leadIn = 1f;

        [SerializeField]
        private float tailOut = 0.25f;

        [SerializeField]
        private RhythmTimingWindows windows;

        [SerializeField]
        private RhythmNote[] notes = Array.Empty<RhythmNote>();

        public float LeadIn => leadIn;
        public float TailOut => tailOut;
        public RhythmTimingWindows Windows => windows;
        public IReadOnlyList<RhythmNote> Notes => notes;

        /// <summary>Builds an in-memory sequence (not saved as an asset) — for tests and procedurally generated sequences. Caller owns destroying it.</summary>
        public static RhythmSequenceDefinition CreateRuntime(float leadIn, float tailOut, RhythmTimingWindows windows, RhythmNote[] notes)
        {
            var definition = CreateInstance<RhythmSequenceDefinition>();
            definition.leadIn = leadIn;
            definition.tailOut = tailOut;
            definition.windows = windows;
            definition.notes = notes;
            return definition;
        }

        /// <summary>Total seconds the sequence runs for, from Start() to the final Completed event.</summary>
        public float Duration
        {
            get
            {
                var maxNoteEnd = 0f;
                foreach (var note in notes)
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

        private void OnValidate()
        {
            for (var i = 0; i < notes.Length; i++)
            {
                if (notes[i].time < 0f)
                {
                    notes[i].time = 0f;
                }

                if (notes[i].holdDuration < 0f)
                {
                    notes[i].holdDuration = 0f;
                }
            }

            Array.Sort(notes, (a, b) => a.time.CompareTo(b.time));

            for (var i = 0; i < notes.Length; i++)
            {
                for (var j = i + 1; j < notes.Length; j++)
                {
                    if (!(notes[i].input == notes[j].input))
                    {
                        continue;
                    }

                    var aMin = notes[i].time - windows.ok;
                    var aMax = notes[i].time + windows.ok;
                    var bMin = notes[j].time - windows.ok;
                    var bMax = notes[j].time + windows.ok;

                    if (aMin < bMax && bMin < aMax)
                    {
                        Debug.LogWarning($"[{name}] RhythmSequenceDefinition: notes {i} and {j} share an input token and have overlapping timing windows.", this);
                    }
                }
            }
        }
    }
}
