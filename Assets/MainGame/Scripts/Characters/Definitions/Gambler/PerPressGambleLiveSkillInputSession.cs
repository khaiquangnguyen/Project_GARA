using System;
using System.Collections.Generic;
using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // Each hit QTE plays the card's move on the spot with one press of its
    // IPerPressGame, landing the roll effects if it landed and whiffing if
    // not. A missed QTE plays nothing; its press goes to the next hit.
    internal sealed class PerPressGambleLiveSkillInputSession : ISequentialLiveSkillInputSession
    {
        private static readonly IGambleRandom Rng = new UnityGambleRandom();

        private readonly QtePlayer _player;
        private readonly Qte _qte;
        private readonly bool _isPlayerControlled;
        private readonly PerPressGambleSkillCard _card;
        private readonly IPerPressGame _game;
        private readonly SkillPerformanceTiering _tiering;
        private readonly List<bool> _landed = new List<bool>();

        private Action<SkillPerformance> _onCompleted;
        private QteRunner _runner;

        public event Action<SkillStep> StepPerformed;

        // Every press plays from where the first one did.
        public event Action<int, AttackAnimationSpec> StepStarting
        {
            add { }
            remove { }
        }

        public AttackAnimationSpec OpeningMove => _card.animationSpec;

        public PerPressGambleLiveSkillInputSession(QtePlayer player, Qte qte, bool isPlayerControlled, PerPressGambleSkillCard card, SkillPerformanceTiering tiering)
        {
            _player = player;
            _qte = qte;
            _isPlayerControlled = isPlayerControlled;
            _card = card;
            _game = card.game as IPerPressGame;
            _tiering = tiering;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            _onCompleted = onCompleted;
            var count = _game != null ? _card.QteCountFor(_game) : 0;
            if (_player == null || count <= 0)
            {
                Complete(SkillPerformance.Failed());
                return;
            }

            _player.Play(_qte, count, _isPlayerControlled, OnInputEnded);
            _runner = _player.CurrentRunner;
            _runner.PromptResolved += OnPromptResolved;
        }

        public void Abort()
        {
            // Fires OnInputEnded with an aborted report.
            _player?.Abort();
        }

        private void OnPromptResolved(QtePrompt prompt)
        {
            if (!prompt.Hit)
            {
                return;
            }

            _landed.Add(_game.RollPress(_landed, Rng));
            var press = _landed.Count - 1;
            var outcome = OutcomeSoFar();
            var performance = PerformanceFor(null, outcome);
            if (_landed[press])
            {
                StepPerformed?.Invoke(new SkillStep(_card.animationSpec, performance, LandingEffects(outcome, performance, press), false, press));
            }
            else
            {
                StepPerformed?.Invoke(new SkillStep(_card.animationSpec, performance, Array.Empty<ISkillEffect>(), false, press, true));
            }
        }

        private void OnInputEnded(QteReport report)
        {
            if (_runner != null)
            {
                _runner.PromptResolved -= OnPromptResolved;
                _runner = null;
            }

            var outcome = OutcomeSoFar();
            if (report.WasAborted)
            {
                Complete(SkillPerformance.Failed(new GambleReport(report, outcome)));
                return;
            }

            GambleEvents.RaiseRolled(_card, outcome, false);
            Complete(PerformanceFor(report, outcome));
        }

        private List<ISkillEffect> LandingEffects(GambleOutcome outcome, SkillPerformance performance, int press)
        {
            var effects = new List<ISkillEffect>();
            foreach (var effect in _card.rollEffects)
            {
                if (effect != null && effect.IsTriggered(outcome, performance))
                {
                    effects.Add(effect.ForStrike(press));
                }
            }

            return effects;
        }

        private GambleOutcome OutcomeSoFar()
        {
            return _game.OutcomeOf(_landed.ToArray());
        }

        private SkillPerformance PerformanceFor(QteReport report, GambleOutcome outcome)
        {
            var score = outcome.Significance;
            return new SkillPerformance(score, _tiering.Evaluate(score), false, new GambleReport(report, outcome));
        }

        private void Complete(SkillPerformance performance)
        {
            var onCompleted = _onCompleted;
            _onCompleted = null;
            onCompleted?.Invoke(performance);
        }
    }
}
