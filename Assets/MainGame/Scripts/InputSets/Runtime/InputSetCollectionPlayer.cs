using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// MonoBehaviour driver that polls an <see cref="InputTokenMap"/> and feeds presses into
    /// an <see cref="InputSetCollectionRunner"/>.
    /// </summary>
    public class InputSetCollectionPlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private InputSetCollectionRunner _runner;
        private Action<InputSetCompletionReport> _onCompleted;

        public event Action<InputSetCompletionReport> SetCollectionCompleted;

        /// <summary>Raised by any player when it starts a set collection, so scene-level visuals can present it without referencing the character.</summary>
        public static event Action<InputSetCollectionRunner> AnySetCollectionStarted;

        /// <summary>Raised by any player once its set collection completes or is aborted.</summary>
        public static event Action<InputSetCollectionRunner> AnySetCollectionEnded;

        public bool IsPlaying => _runner != null && _runner.IsRunning;

        public InputSetCollectionRunner CurrentRunner => _runner;

        public void Play(InputSetCollectionDefinition definition, Action<InputSetCompletionReport> onCompleted = null)
        {
            Abort();

            _poller = new InputTokenPoller(inputMap);
            _runner = new InputSetCollectionRunner(definition);
            _onCompleted = onCompleted;
            _runner.Completed += HandleCompleted;

            inputMap?.EnableAll();

            // Raised before Start() so visuals are bound when Start() fires the first SetStarted
            // (and, for an empty set collection, Completed — which raises AnySetCollectionEnded in order).
            AnySetCollectionStarted?.Invoke(_runner);
            _runner.Start();
        }

        public void Abort()
        {
            if (_runner == null)
            {
                return;
            }

            if (_runner.IsRunning)
            {
                _runner.Abort();
            }

            _runner.Completed -= HandleCompleted;
            _runner = null;
            _onCompleted = null;
        }

        private void Update()
        {
            if (_runner == null || !_runner.IsRunning)
            {
                return;
            }

            _runner.Tick(Time.deltaTime);

            if (!_runner.IsRunning)
            {
                return;
            }

            _poller.Poll(HandlePressed, null);
        }

        private void HandlePressed(InputToken token)
        {
            _runner?.Press(token);
        }

        private void HandleCompleted(InputSetCompletionReport report)
        {
            AnySetCollectionEnded?.Invoke(_runner);
            _onCompleted?.Invoke(report);
            SetCollectionCompleted?.Invoke(report);
        }

        private void OnDisable()
        {
            Abort();
        }
    }
}
