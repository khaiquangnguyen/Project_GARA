using System;
using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // One QTE, then the game rolled, cheated if it was hit; the card's
    // move plays once per strike (GamblerSkillCard.StrikesFor), one after
    // another, each with the effects the roll triggered.
    internal sealed class GamblerLiveSkillInputSession : ISequentialLiveSkillInputSession
    {
        private static readonly IGambleRandom Rng = new UnityGambleRandom();

        private readonly QtePlayer _player;
        private readonly Qte _qte;
        private readonly bool _isPlayerControlled;
        private readonly MonoBehaviour _runner;
        private readonly GamblerSkillCard _card;
        private readonly SkillPerformanceTiering _tiering;
        private readonly float _revealSeconds;

        private Action<SkillPerformance> _onCompleted;
        private Coroutine _strikes;
        private SkillPerformance _performance;

        public event Action<SkillStep> StepPerformed;

        // Every strike plays from where the first one did.
        public event Action<int, AttackAnimationSpec> StepStarting
        {
            add { }
            remove { }
        }

        public AttackAnimationSpec OpeningMove => _card.animationSpec;

        public GamblerLiveSkillInputSession(QtePlayer player, Qte qte, bool isPlayerControlled, MonoBehaviour runner, GamblerSkillCard card, SkillPerformanceTiering tiering, float revealSeconds)
        {
            _player = player;
            _qte = qte;
            _isPlayerControlled = isPlayerControlled;
            _runner = runner;
            _card = card;
            _tiering = tiering;
            _revealSeconds = card.showOutcome ? revealSeconds : 0f;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            _onCompleted = onCompleted;
            if (_player == null || _runner == null || _card.game == null)
            {
                Complete(SkillPerformance.Failed());
                return;
            }

            _player.Play(_qte, 1, _isPlayerControlled, OnInputEnded);
        }

        public void Abort()
        {
            // Fires OnInputEnded with an aborted report mid-run.
            _player?.Abort();
            if (_strikes == null)
            {
                return;
            }

            _runner.StopCoroutine(_strikes);
            _strikes = null;
            Complete(SkillPerformance.Failed(_performance.Details));
        }

        private void OnInputEnded(QteReport qte)
        {
            if (qte.WasAborted)
            {
                Complete(SkillPerformance.Failed(new GambleReport(qte, null)));
                return;
            }

            var cheated = qte.AllHit;
            var score = cheated ? 1f : 0f;
            var outcome = _card.game.Roll(cheated, Rng);
            _performance = new SkillPerformance(score, _tiering.Evaluate(score), false, new GambleReport(qte, outcome));
            var triggered = Triggered(outcome, _performance);
            var strikes = triggered != null ? _card.StrikesFor(outcome) : 0;
            Debug.Log($"[{nameof(GamblerLiveSkillInputSession)}] {_card.name}: {(cheated ? "cheated" : "fair")} — {outcome} — {strikes} strike(s)");
            GambleEvents.RaiseRolled(_card, outcome, cheated);
            if (strikes <= 1 && _revealSeconds <= 0f)
            {
                if (strikes == 1)
                {
                    Strike(triggered, 0);
                }

                Complete(_performance);
                return;
            }

            _strikes = _runner.StartCoroutine(StrikeRepeatedly(triggered, strikes));
        }

        // Shows the roll first, then strikes.
        private IEnumerator StrikeRepeatedly(IReadOnlyList<ISkillEffect> triggered, int strikes)
        {
            if (_revealSeconds > 0f)
            {
                yield return new WaitForSeconds(_revealSeconds);
            }

            for (var index = 0; index < strikes; index++)
            {
                if (index > 0)
                {
                    yield return new WaitForSeconds(_card.SecondsBetweenStrikes);
                }

                Strike(triggered, index);
            }

            _strikes = null;
            Complete(_performance);
        }

        private void Strike(IReadOnlyList<ISkillEffect> triggered, int index)
        {
            var effects = new List<ISkillEffect>(triggered.Count);
            foreach (var effect in triggered)
            {
                effects.Add(effect is GambleEffect gamble ? gamble.ForStrike(index) : effect);
            }

            StepPerformed?.Invoke(new SkillStep(_card.animationSpec, _performance, effects, false, index));
        }

        private void Complete(SkillPerformance performance)
        {
            var onCompleted = _onCompleted;
            _onCompleted = null;
            onCompleted?.Invoke(performance);
        }

        // Null when none pass.
        private List<ISkillEffect> Triggered(GambleOutcome outcome, SkillPerformance performance)
        {
            List<ISkillEffect> triggered = null;
            foreach (var effect in _card.rollEffects)
            {
                if (effect != null && effect.IsTriggered(outcome, performance))
                {
                    triggered ??= new List<ISkillEffect>();
                    triggered.Add(effect);
                }
            }

            return triggered;
        }
    }
}
