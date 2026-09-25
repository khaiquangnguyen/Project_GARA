using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>MonoBehaviour host that drives a <see cref="RhythmSequenceRunner"/> from Update().</summary>
    public class RhythmSequencePlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        [SerializeField]
        private bool useUnscaledTime;

        private InputTokenPoller _poller;
        private RhythmSequenceRunner _currentRunner;
        private Action<InputToken> _onTokenPressed;
        private Action<InputToken> _onTokenReleased;
        private Action<RhythmCompletionReport> _onCompletedCallback;

        public bool IsPlaying => _currentRunner != null && _currentRunner.IsRunning;
        public RhythmSequenceRunner CurrentRunner => _currentRunner;

        public event Action<RhythmCompletionReport> SequenceCompleted;

        /// <summary>Raised by any player when it starts a sequence, so scene-level visuals can present it without referencing the character.</summary>
        public static event Action<RhythmSequenceRunner> AnySequenceStarted;

        /// <summary>Raised by any player once its sequence completes or is aborted.</summary>
        public static event Action<RhythmSequenceRunner> AnySequenceEnded;

        public void Play(RhythmSequenceDefinition definition)
        {
            Play(definition, null);
        }

        public void Play(RhythmSequenceDefinition definition, Action<RhythmCompletionReport> onCompleted)
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

            _currentRunner = new RhythmSequenceRunner(definition);
            _onCompletedCallback = onCompleted;
            _currentRunner.Completed += HandleCompleted;

            inputMap?.EnableAll();
            _currentRunner.Start();
            AnySequenceStarted?.Invoke(_currentRunner);
        }

        public void Abort()
        {
            _currentRunner?.Abort();
        }

        private void Awake()
        {
            _onTokenPressed = HandleTokenPressed;
            _onTokenReleased = HandleTokenReleased;
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _poller.Poll(_onTokenPressed, _onTokenReleased);
            _currentRunner.Tick(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void OnDisable()
        {
            if (IsPlaying)
            {
                _currentRunner.Abort();
            }
        }

        private void HandleTokenPressed(InputToken token)
        {
            _currentRunner?.Press(token);
        }

        private void HandleTokenReleased(InputToken token)
        {
            _currentRunner?.Release(token);
        }

        private void HandleCompleted(RhythmCompletionReport report)
        {
            _currentRunner.Completed -= HandleCompleted;
            AnySequenceEnded?.Invoke(_currentRunner);
            SequenceCompleted?.Invoke(report);
            _onCompletedCallback?.Invoke(report);
            _onCompletedCallback = null;
        }
    }
}
