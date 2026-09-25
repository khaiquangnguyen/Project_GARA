using System.Collections.Generic;

namespace GARA.Rhythm
{
    /// <summary>Aggregated scoring for a finished (or aborted) rhythm sequence run.</summary>
    public sealed class RhythmCompletionReport
    {
        public int TotalNotes { get; }
        public int HitNotes { get; }
        public int PerfectCount { get; }
        public int GoodCount { get; }
        public int OkCount { get; }
        public int MissCount { get; }
        public int StrayPresses { get; }
        public float CompletionRate { get; }
        public float AccuracyScore { get; }
        public bool WasAborted { get; }
        public IReadOnlyList<RhythmNoteResult> NoteResults { get; }

        public RhythmCompletionReport(IReadOnlyList<RhythmNoteResult> noteResults, int strayPresses, bool wasAborted)
        {
            NoteResults = noteResults;
            StrayPresses = strayPresses;
            WasAborted = wasAborted;
            TotalNotes = noteResults.Count;

            var weightSum = 0f;
            foreach (var result in noteResults)
            {
                switch (result.Judgement)
                {
                    case RhythmJudgement.Perfect:
                        PerfectCount++;
                        HitNotes++;
                        weightSum += 1f * result.HoldCompletion;
                        break;
                    case RhythmJudgement.Good:
                        GoodCount++;
                        HitNotes++;
                        weightSum += 0.75f * result.HoldCompletion;
                        break;
                    case RhythmJudgement.Ok:
                        OkCount++;
                        HitNotes++;
                        weightSum += 0.5f * result.HoldCompletion;
                        break;
                    default:
                        MissCount++;
                        break;
                }
            }

            CompletionRate = TotalNotes > 0 ? (float)HitNotes / TotalNotes : 0f;
            AccuracyScore = TotalNotes > 0 ? weightSum / TotalNotes : 0f;
        }
    }
}
