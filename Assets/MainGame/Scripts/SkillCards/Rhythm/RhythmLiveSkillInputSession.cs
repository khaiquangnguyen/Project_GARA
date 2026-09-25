using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.Rhythm;

namespace GARA.SkillCards.Rhythm
{
    // Rhythm session for a card whose bars have moves: each bar
    // whose notes are all hit emits its move as a step the moment its last
    // note is judged; a run with no missed note ends with the finale step
    // (the state's own clip).
    internal sealed class RhythmLiveSkillInputSession : ILiveSkillInputSession
    {
        private readonly RhythmSequencePlayer _player;
        private readonly RhythmSkillCard _card;

        private RhythmSequenceRunner _runner;
        private int[] _judgedCount;
        private bool[] _allHit;
        private float[] _scoreSum;
        private List<RhythmNoteResult>[] _barResults;

        public event Action<SkillStep> StepPerformed;

        public AttackAnimationSpec OpeningMove => _card.OpeningMove;

        public RhythmLiveSkillInputSession(RhythmSequencePlayer player, RhythmSkillCard card)
        {
            _player = player;
            _card = card;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null || _card.Sequence == null)
            {
                onCompleted(SkillPerformance.Failed());
                return;
            }

            var barCount = _card.Sequence.Bars.Count;
            _judgedCount = new int[barCount];
            _allHit = new bool[barCount];
            _scoreSum = new float[barCount];
            _barResults = new List<RhythmNoteResult>[barCount];
            for (var b = 0; b < barCount; b++)
            {
                _allHit[b] = true;
                _barResults[b] = new List<RhythmNoteResult>();
            }

            _player.Play(_card.Sequence, report => Complete(report, onCompleted));
            _runner = _player.CurrentRunner;
            if (_runner != null)
            {
                _runner.NoteJudged += OnNoteJudged;
            }
        }

        public void Abort()
        {
            _player?.Abort();
        }

        private void OnNoteJudged(RhythmNoteResult result)
        {
            var sequence = _card.Sequence;
            var barIndex = sequence.BarOfNote(result.NoteIndex);
            var bar = sequence.Bars[barIndex];

            _judgedCount[barIndex]++;
            _allHit[barIndex] &= result.IsHit;
            _scoreSum[barIndex] += ScoreOf(result.Judgement);
            _barResults[barIndex].Add(result);

            if (_judgedCount[barIndex] < bar.notes.Length || !_allHit[barIndex])
            {
                return;
            }

            if (!_card.TryGetMove(barIndex, out var move))
            {
                return;
            }

            var score = _scoreSum[barIndex] / bar.notes.Length;
            var performance = new SkillPerformance(score, _card.TierFor(score), false, _barResults[barIndex]);
            StepPerformed?.Invoke(new SkillStep(move, performance, 1f / sequence.Bars.Count, false));
        }

        private void Complete(RhythmCompletionReport report, Action<SkillPerformance> onCompleted)
        {
            if (_runner != null)
            {
                _runner.NoteJudged -= OnNoteJudged;
                _runner = null;
            }

            var performance = RhythmPerformanceMapper.Map(report, _card);
            // Perfect execution is hit/miss: any Perfect, Good or Ok counts.
            var isPerfect = !report.WasAborted && report.TotalNotes > 0 && report.MissCount == 0;
            UnityEngine.Debug.Log($"[{nameof(RhythmLiveSkillInputSession)}] {_card.name}: {report.HitNotes}/{report.TotalNotes} hit " +
                                  $"(P={report.PerfectCount} G={report.GoodCount} O={report.OkCount}) — {(isPerfect ? "finale" : "no finale")}");
            if (isPerfect)
            {
                StepPerformed?.Invoke(new SkillStep(null, performance, 1f, true));
            }

            onCompleted(performance);
        }

        private static float ScoreOf(RhythmJudgement judgement)
        {
            switch (judgement)
            {
                case RhythmJudgement.Perfect:
                    return 1f;
                case RhythmJudgement.Good:
                    return 0.75f;
                case RhythmJudgement.Ok:
                    return 0.5f;
                default:
                    return 0f;
            }
        }
    }
}
