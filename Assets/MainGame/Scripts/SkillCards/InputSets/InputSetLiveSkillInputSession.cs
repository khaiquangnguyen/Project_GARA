using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.InputSets;

namespace GARA.SkillCards.InputSets
{
    // Emits each step's move when its set finishes, if one of its effects'
    // gates passes on that set; the finale likewise, gated on the whole run.
    internal sealed class InputSetLiveSkillInputSession : ILiveSkillInputSession
    {
        private readonly InputSetCollectionPlayer _player;
        private readonly LiveInputSetSkillCard _card;
        private readonly SkillPerformanceTiering _tiering;
        private readonly InputSetScoreModel _scoreModel;

        private InputSetCollectionRunner _runner;

        public event Action<SkillStep> StepPerformed;

        public AttackAnimationSpec OpeningMove => _card.OpeningMove;

        public InputSetLiveSkillInputSession(InputSetCollectionPlayer player, LiveInputSetSkillCard card, SkillPerformanceTiering tiering, InputSetScoreModel scoreModel)
        {
            _player = player;
            _card = card;
            _tiering = tiering;
            _scoreModel = scoreModel;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null || _card.Collection == null)
            {
                onCompleted(SkillPerformance.Failed());
                return;
            }

            _player.Play(_card.Collection, report => Complete(report, onCompleted));
            _runner = _player.CurrentRunner;
            if (_runner != null)
            {
                _runner.SetFinished += OnSetFinished;
            }
        }

        public void Abort()
        {
            _player?.Abort();
        }

        private void OnSetFinished(InputSetResult result)
        {
            var step = _card.Steps[result.SetIndex];
            if (step.move == null)
            {
                return;
            }

            var stepReport = InputSetCompletionReport.ForSingleSet(result);
            var triggered = Triggered(step.effects, stepReport);
            if (triggered == null)
            {
                return;
            }

            StepPerformed?.Invoke(new SkillStep(step.move, _card.BuildPerformance(stepReport, _tiering, _scoreModel), triggered, false));
        }

        private void Complete(InputSetCompletionReport report, Action<SkillPerformance> onCompleted)
        {
            if (_runner != null)
            {
                _runner.SetFinished -= OnSetFinished;
                _runner = null;
            }

            var performance = _card.BuildPerformance(report, _tiering, _scoreModel);
            var triggered = report.WasAborted ? null : Triggered(_card.finaleEffects, report);
            UnityEngine.Debug.Log($"[{nameof(InputSetLiveSkillInputSession)}] {_card.name}: {report.ClearedSets}/{report.TotalSets} cleared " +
                                  $"({report.FirstTryClears} first try) — {(triggered != null ? "finale" : "no finale")}");
            if (triggered != null)
            {
                StepPerformed?.Invoke(new SkillStep(null, performance, triggered, true));
            }

            onCompleted(performance);
        }

        // Null when none pass.
        private static List<ISkillEffect> Triggered(InputSetStepEffect[] effects, InputSetCompletionReport report)
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
