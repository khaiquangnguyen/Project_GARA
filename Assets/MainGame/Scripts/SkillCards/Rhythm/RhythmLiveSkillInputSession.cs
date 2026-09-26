using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.Rhythm;

namespace GARA.SkillCards.Rhythm
{
    // Each bar emits its move as a step the moment its last note is judged,
    // if any of its effects' gates pass on its notes; the finale (the
    // state's own clip) likewise if any finale effect's gate passes on the
    // whole run. Only the triggered effects go with the step.
    internal sealed class RhythmLiveSkillInputSession : ILiveSkillInputSession
    {
        private readonly RhythmSequencePlayer _player;
        private readonly RhythmSkillCard _card;
        private readonly SkillPerformanceTiering _tiering;

        private RhythmSequenceRunner _runner;
        private int[] _judgedCount;
        private List<RhythmNoteResult>[] _barResults;

        public event Action<SkillStep> StepPerformed;

        public AttackAnimationSpec OpeningMove => _card.OpeningMove;

        public RhythmLiveSkillInputSession(RhythmSequencePlayer player, RhythmSkillCard card, SkillPerformanceTiering tiering)
        {
            _player = player;
            _card = card;
            _tiering = tiering;
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
            _barResults = new List<RhythmNoteResult>[barCount];
            for (var b = 0; b < barCount; b++)
            {
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
            var bar = _card.Bars[barIndex];

            _judgedCount[barIndex]++;
            _barResults[barIndex].Add(result);

            if (_judgedCount[barIndex] < sequence.Bars[barIndex].notes.Length || bar.move == null)
            {
                return;
            }

            // Scored like a whole run, over this bar's notes only.
            var barReport = new RhythmCompletionReport(_barResults[barIndex], 0, false);
            var triggered = Triggered(bar.effects, barReport);
            if (triggered == null)
            {
                return;
            }

            StepPerformed?.Invoke(new SkillStep(bar.move, RhythmPerformanceMapper.Map(barReport, _tiering), triggered, false));
        }

        private void Complete(RhythmCompletionReport report, Action<SkillPerformance> onCompleted)
        {
            if (_runner != null)
            {
                _runner.NoteJudged -= OnNoteJudged;
                _runner = null;
            }

            var performance = RhythmPerformanceMapper.Map(report, _tiering);
            var triggered = report.WasAborted ? null : Triggered(_card.finaleEffects, report);
            UnityEngine.Debug.Log($"[{nameof(RhythmLiveSkillInputSession)}] {_card.name}: {report.HitNotes}/{report.TotalNotes} hit " +
                                  $"(P={report.PerfectCount} G={report.GoodCount} O={report.OkCount}) — {(triggered != null ? "finale" : "no finale")}");
            if (triggered != null)
            {
                StepPerformed?.Invoke(new SkillStep(null, performance, triggered, true));
            }

            onCompleted(performance);
        }

        // The effects whose gates pass on report; null when none do.
        private static List<ISkillEffect> Triggered(RhythmStepEffect[] effects, RhythmCompletionReport report)
        {
            if (effects == null)
            {
                return null;
            }

            List<ISkillEffect> triggered = null;
            foreach (var effect in effects)
            {
                if (effect != null && effect.IsTriggered(report))
                {
                    triggered ??= new List<ISkillEffect>();
                    triggered.Add(effect);
                }
            }

            return triggered;
        }
    }
}
