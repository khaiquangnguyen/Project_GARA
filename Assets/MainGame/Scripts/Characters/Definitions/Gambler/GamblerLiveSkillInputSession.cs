using System;
using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The cheat shake, then the game rolled, cheated if it landed; the card's
    // move plays once per strike (GamblerSkillCard.StrikesFor), each with
    // the effects the roll triggered.
    internal sealed class GamblerLiveSkillInputSession : ILiveSkillInputSession
    {
        private static readonly IGambleRandom Rng = new UnityGambleRandom();

        private readonly CheatShakePlayer _player;
        private readonly CheatShake _cheat;
        private readonly bool _isPlayerControlled;
        private readonly MonoBehaviour _runner;
        private readonly GamblerSkillCard _card;
        private readonly SkillPerformanceTiering _tiering;

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

        public GamblerLiveSkillInputSession(CheatShakePlayer player, CheatShake cheat, bool isPlayerControlled, MonoBehaviour runner, GamblerSkillCard card, SkillPerformanceTiering tiering)
        {
            _player = player;
            _cheat = cheat;
            _isPlayerControlled = isPlayerControlled;
            _runner = runner;
            _card = card;
            _tiering = tiering;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            _onCompleted = onCompleted;
            if (_player == null || _runner == null || _card.game == null)
            {
                Complete(SkillPerformance.Failed());
                return;
            }

            _player.Play(_cheat, _isPlayerControlled, OnCheatEnded);
        }

        public void Abort()
        {
            // Fires OnCheatEnded with an aborted report mid-run.
            _player?.Abort();
            if (_strikes == null)
            {
                return;
            }

            _runner.StopCoroutine(_strikes);
            _strikes = null;
            Complete(SkillPerformance.Failed(_performance.Details));
        }

        private void OnCheatEnded(CheatShakeReport cheat)
        {
            if (cheat.WasAborted)
            {
                Complete(SkillPerformance.Failed(new GambleReport(cheat, null)));
                return;
            }

            var cheated = cheat.Cleared;
            var score = cheated ? 1f : 0f;
            var outcome = _card.game.Roll(cheated, Rng);
            _performance = new SkillPerformance(score, _tiering.Evaluate(score), false, new GambleReport(cheat, outcome));
            var triggered = Triggered(outcome, _performance);
            var strikes = triggered != null ? _card.StrikesFor(outcome) : 0;
            Debug.Log($"[{nameof(GamblerLiveSkillInputSession)}] {_card.name}: {(cheated ? "cheated" : "fair")} — {outcome} — {strikes} strike(s)");
            if (strikes <= 1)
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

        private IEnumerator StrikeRepeatedly(IReadOnlyList<ISkillEffect> triggered, int strikes)
        {
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
            StepPerformed?.Invoke(new SkillStep(_card.animationSpec, _performance, triggered, false, index));
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
