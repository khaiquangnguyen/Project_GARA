using System;
using GARA.Characters;
using GARA.Shake;

namespace GARA.SkillCards.Shake
{
    // One run of a ShakeSkillCard's minigame against a ShakeInputPlayer
    // driver. Distinguishes a target-reached early stop (still a completed
    // run) from an externally requested Abort() via a private flag, since
    // both paths call ShakeInputPlayer.Stop() under the hood.
    public sealed class ShakeSkillInputSession : ISkillInputSession
    {
        private readonly ShakeSkillCard _card;
        private readonly ShakeInputPlayer _player;

        private bool _aborted;
        private Action<SkillPerformance> _onCompleted;

        public ShakeSkillInputSession(ShakeSkillCard card, ShakeInputPlayer player)
        {
            _card = card;
            _player = player;
        }

        public int CurrentPairs => _player != null && _player.CurrentRunner != null ? _player.CurrentRunner.CompletedPairs : 0;

        public float CurrentProgress => ShakePerformanceMapper.ComputeProgress(
            CurrentPairs,
            _player != null && _player.CurrentRunner != null ? _player.CurrentRunner.WrongPresses : 0,
            _card.TargetPairs,
            _card.WrongPressPenalty);

        public event Action<int> PairCompleted;

        public void Begin(Action<SkillPerformance> onCompleted)
        {
            if (_player == null || _card.ShakeInput == null)
            {
                onCompleted(SkillPerformance.Failed());
                return;
            }

            _aborted = false;
            _onCompleted = onCompleted;

            _player.Play(_card.ShakeInput, HandleReport);
            _player.CurrentRunner.PairCompleted += HandlePairCompleted;
        }

        public void Abort()
        {
            _aborted = true;
            _player?.Stop();
        }

        private void HandlePairCompleted(int completedPairs)
        {
            PairCompleted?.Invoke(completedPairs);

            if (_card.StopWhenTargetReached && completedPairs >= _card.TargetPairs)
            {
                _player.Stop();
            }
        }

        private void HandleReport(ShakeInputReport report)
        {
            if (_player.CurrentRunner != null)
            {
                _player.CurrentRunner.PairCompleted -= HandlePairCompleted;
            }

            var performance = ShakePerformanceMapper.Map(report, _card, _aborted);
            var callback = _onCompleted;
            _onCompleted = null;
            callback?.Invoke(performance);
        }
    }
}
