using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.ShakeBalance;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Shake balance, then the game rolled with the score as luck; the card's
    // move is the only step, with the effects the roll triggered.
    internal sealed class GamblerLiveSkillInputSession : ILiveSkillInputSession
    {
        private static readonly IGambleRandom Rng = new UnityGambleRandom();

        private readonly ShakeBalancePlayer _player;
        private readonly GamblerSkillCard _card;
        private readonly SkillPerformanceTiering _tiering;

        public event Action<SkillStep> StepPerformed;

        public AttackAnimationSpec OpeningMove => _card.animationSpec;

        public GamblerLiveSkillInputSession(ShakeBalancePlayer player, GamblerSkillCard card, SkillPerformanceTiering tiering)
        {
            _player = player;
            _card = card;
            _tiering = tiering;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null || _card.Balance == null || _card.game == null)
            {
                onCompleted(SkillPerformance.Failed());
                return;
            }

            _player.Play(_card.Balance, report => Complete(report, onCompleted));
        }

        public void Abort()
        {
            _player?.Abort();
        }

        private void Complete(ShakeBalanceReport balance, Action<SkillPerformance> onCompleted)
        {
            if (balance.WasAborted)
            {
                onCompleted(SkillPerformance.Failed(new GambleReport(balance, null)));
                return;
            }

            var luck = Mathf.Clamp01(_card.ScoreShaping.Evaluate(balance.Result));
            var outcome = _card.game.Roll(luck, Rng);
            var performance = new SkillPerformance(luck, _tiering.Evaluate(luck), false, new GambleReport(balance, outcome));
            var triggered = Triggered(outcome, performance);
            Debug.Log($"[{nameof(GamblerLiveSkillInputSession)}] {_card.name}: luck {luck:0.00} — {outcome} — {(triggered != null ? "hit" : "no hit")}");
            if (triggered != null)
            {
                StepPerformed?.Invoke(new SkillStep(_card.animationSpec, performance, triggered, false));
            }

            onCompleted(performance);
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
