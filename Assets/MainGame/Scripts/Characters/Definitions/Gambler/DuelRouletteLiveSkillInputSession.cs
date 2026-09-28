using System;
using System.Collections;
using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // The cheat shake, then the round pull by pull: an empty click is
    // a step with no effects; the ending is a step with the triggered roll
    // effects. The AI-controlled Gambler never chickens out.
    internal sealed class DuelRouletteLiveSkillInputSession : ILiveSkillInputSession
    {
        private static readonly IGambleRandom Rng = new UnityGambleRandom();

        private readonly CheatShakePlayer _cheatPlayer;
        private readonly CheatShake _cheat;
        private readonly DuelRouletteDecisionPlayer _decisionPlayer;
        private readonly MonoBehaviour _runner;
        private readonly DuelRouletteSkillCard _card;
        private readonly SkillPerformanceTiering _tiering;
        private readonly bool _isPlayerControlled;

        private Action<SkillPerformance> _onCompleted;
        private Coroutine _round;
        private SkillPerformance _performance;
        private int _stepIndex;

        public event Action<SkillStep> StepPerformed;

        // Nothing to get into position for between pulls.
        public event Action<int, AttackAnimationSpec> StepStarting
        {
            add { }
            remove { }
        }

        public AttackAnimationSpec OpeningMove => _card.animationSpec;

        public DuelRouletteLiveSkillInputSession(CheatShakePlayer cheatPlayer, CheatShake cheat, DuelRouletteDecisionPlayer decisionPlayer, MonoBehaviour runner, DuelRouletteSkillCard card, SkillPerformanceTiering tiering, bool isPlayerControlled)
        {
            _cheatPlayer = cheatPlayer;
            _cheat = cheat;
            _decisionPlayer = decisionPlayer;
            _runner = runner;
            _card = card;
            _tiering = tiering;
            _isPlayerControlled = isPlayerControlled;
        }

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            _onCompleted = onCompleted;
            if (_cheatPlayer == null || _runner == null || !(_card.game is DuelRouletteGame))
            {
                Complete(SkillPerformance.Failed());
                return;
            }

            _cheatPlayer.Play(_cheat, _isPlayerControlled, OnCheatEnded);
        }

        public void Abort()
        {
            // Fires OnCheatEnded with an aborted report mid-run.
            _cheatPlayer?.Abort();
            if (_round == null)
            {
                return;
            }

            _runner.StopCoroutine(_round);
            _round = null;
            _decisionPlayer?.Cancel();
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
            var round = (DuelRouletteOutcome)_card.game.Roll(cheated, Rng);
            _performance = new SkillPerformance(score, _tiering.Evaluate(score), false, new GambleReport(cheat, round));
            _round = _runner.StartCoroutine(Play(round));
        }

        private IEnumerator Play(DuelRouletteOutcome round)
        {
            var game = (DuelRouletteGame)_card.game;
            while (true)
            {
                var seat = round.NextSeat;
                var pull = true;
                if (seat == DuelSeat.Target)
                {
                    pull = game.TargetPulls(round);
                }
                else if (_isPlayerControlled && _decisionPlayer != null && round.CanChickenOut(seat))
                {
                    var decided = false;
                    _decisionPlayer.Ask(round, choice =>
                    {
                        pull = choice;
                        decided = true;
                    });
                    yield return new WaitUntil(() => decided);
                }

                if (!pull)
                {
                    round.ChickenOut();
                    break;
                }

                if (round.Pull())
                {
                    break;
                }

                Emit(seat == DuelSeat.Gambler ? _card.gamblerPullSpec : _card.targetPullSpec, null);
                yield return new WaitForSeconds(_card.secondsPerPull);
            }

            _round = null;
            Debug.Log($"[{nameof(DuelRouletteLiveSkillInputSession)}] {_card.name}: {(_performance.Score >= 1f ? "cheated" : "fair")} — {round}");
            Emit(EndingSpec(round), Triggered(round));
            Complete(_performance);
        }

        private AttackAnimationSpec EndingSpec(DuelRouletteOutcome round)
        {
            var shot = round.Ending == DuelEnding.Shot;
            if (round.EndedBy == DuelSeat.Gambler)
            {
                return shot ? _card.gamblerShotSpec : _card.gamblerChickenOutSpec;
            }

            return shot ? _card.targetShotSpec : _card.targetChickenOutSpec;
        }

        private void Emit(AttackAnimationSpec spec, IReadOnlyList<ISkillEffect> effects)
        {
            StepPerformed?.Invoke(new SkillStep(spec, _performance, effects, false, _stepIndex++));
        }

        // Null when none pass.
        private List<ISkillEffect> Triggered(DuelRouletteOutcome round)
        {
            List<ISkillEffect> triggered = null;
            foreach (var effect in _card.rollEffects)
            {
                if (effect != null && effect.IsTriggered(round, _performance))
                {
                    triggered ??= new List<ISkillEffect>();
                    triggered.Add(effect);
                }
            }

            return triggered;
        }

        private void Complete(SkillPerformance performance)
        {
            var onCompleted = _onCompleted;
            _onCompleted = null;
            onCompleted?.Invoke(performance);
        }
    }
}
