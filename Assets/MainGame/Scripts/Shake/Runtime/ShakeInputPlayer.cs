using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Shake
{
    /// <summary>MonoBehaviour host that drives a <see cref="ShakeInputRunner"/> from Update().</summary>
    public class ShakeInputPlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private ShakeInputRunner _currentRunner;
        private Action<InputToken> _onTokenPressed;
        private Action<ShakeInputReport> _onCompletedCallback;

        public bool IsPlaying => _currentRunner != null && _currentRunner.IsRunning;
        public ShakeInputRunner CurrentRunner => _currentRunner;
        public int CompletedPairs => _currentRunner?.CompletedPairs ?? 0;

        public event Action<ShakeInputReport> ShakeCompleted;

        public int GetCompletedPairCount()
        {
            return CompletedPairs;
        }

        public void Play(ShakeInputDefinition definition, Action<ShakeInputReport> onCompleted = null)
        {
            if (definition == null)
            {
                return;
            }

            if (_currentRunner != null)
            {
                _currentRunner.Completed -= HandleCompleted;
            }

            if (_poller == null)
            {
                _poller = new InputTokenPoller(inputMap);
            }

            _currentRunner = new ShakeInputRunner(definition);
            _onCompletedCallback = onCompleted;
            _currentRunner.Completed += HandleCompleted;
            _currentRunner.Start();
        }

        public void Stop()
        {
            _currentRunner?.Stop();
        }

        private void Awake()
        {
            _onTokenPressed = HandleTokenPressed;
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _poller.Poll(_onTokenPressed, null);
            _currentRunner.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (IsPlaying)
            {
                _currentRunner.Stop();
            }
        }

        private void HandleTokenPressed(InputToken token)
        {
            _currentRunner?.Press(token);
        }

        private void HandleCompleted(ShakeInputReport report)
        {
            _currentRunner.Completed -= HandleCompleted;
            ShakeCompleted?.Invoke(report);
            _onCompletedCallback?.Invoke(report);
            _onCompletedCallback = null;
        }
    }
}
