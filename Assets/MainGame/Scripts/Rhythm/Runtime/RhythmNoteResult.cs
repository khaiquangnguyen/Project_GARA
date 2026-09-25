using GARA.Input;

namespace GARA.Rhythm
{
    /// <summary>The resolved outcome of a single note.</summary>
    public readonly struct RhythmNoteResult
    {
        public int NoteIndex { get; }
        public InputToken Input { get; }
        public RhythmJudgement Judgement { get; }

        /// <summary>Signed seconds from the note's target time (negative = early, positive = late). NaN when Miss.</summary>
        public float DeltaSeconds { get; }

        /// <summary>0..1 how much of a Hold note's duration was held. Always 1 for Tap notes.</summary>
        public float HoldCompletion { get; }

        public bool IsHit => Judgement != RhythmJudgement.Miss;

        public RhythmNoteResult(int noteIndex, InputToken input, RhythmJudgement judgement, float deltaSeconds, float holdCompletion)
        {
            NoteIndex = noteIndex;
            Input = input;
            Judgement = judgement;
            DeltaSeconds = deltaSeconds;
            HoldCompletion = holdCompletion;
        }
    }
}
