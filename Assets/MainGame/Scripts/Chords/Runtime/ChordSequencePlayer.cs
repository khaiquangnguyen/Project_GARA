using System;
using GARA.Input;
using GARA.InputSets;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Chords
{
    /// <summary>
    /// MonoBehaviour driver that polls an <see cref="InputTokenMap"/> and feeds both presses
    /// and releases into a <see cref="ChordSequenceRunner"/>.
    /// </summary>
    public class ChordSequencePlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private ChordSequenceRunner _runner;
        private Action<InputSetCompletionReport> _onCompleted;

        public event Action<InputSetCompletionReport> SequenceCompleted;

        public bool IsPlaying => _runner != null && _runner.IsRunning;

        public ChordSequenceRunner CurrentRunner => _runner;

        public void Play(ChordSequenceDefinition definition, Action<InputSetCompletionReport> onCompleted = null)
        {
            Abort();

            _poller = new InputTokenPoller(inputMap);
            _runner = new ChordSequenceRunner(definition);
            _onCompleted = onCompleted;
            _runner.Completed += HandleCompleted;

            inputMap?.EnableAll();
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

            _poller.Poll(HandlePressed, HandleReleased);
        }

        private void HandlePressed(InputToken token)
        {
            _runner?.Press(token);
        }

        private void HandleReleased(InputToken token)
        {
            _runner?.Release(token);
        }

        private void HandleCompleted(InputSetCompletionReport report)
        {
            _onCompleted?.Invoke(report);
            SequenceCompleted?.Invoke(report);
        }

        private void OnDisable()
        {
            Abort();
        }
    }
}
