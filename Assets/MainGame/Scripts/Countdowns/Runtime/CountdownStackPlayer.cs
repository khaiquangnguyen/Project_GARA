using System;
using System.Collections.Generic;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Countdowns
{
    /// <summary>
    /// MonoBehaviour driver that polls an <see cref="InputTokenMap"/> and ticks a
    /// <see cref="CountdownStackRunner"/> each frame.
    /// </summary>
    public class CountdownStackPlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private Action<CountdownStackReport> _onCompleted;

        public bool IsPlaying => CurrentRunner != null && CurrentRunner.IsRunning;
        public CountdownStackRunner CurrentRunner { get; private set; }

        public int SuccessCount => CurrentRunner?.SuccessCount ?? 0;
        public IReadOnlyList<ActiveCountdown> Active => CurrentRunner?.Active ?? Array.Empty<ActiveCountdown>();

        public event Action<CountdownStackReport> Completed;

        public void Play(CountdownStackDefinition definition, Action<CountdownStackReport> onCompleted = null)
        {
            _poller ??= new InputTokenPoller(inputMap);

            CurrentRunner = new CountdownStackRunner(definition);
            _onCompleted = onCompleted;
            CurrentRunner.Completed += HandleCompleted;
            CurrentRunner.Start();

            inputMap?.EnableAll();
        }

        public void Stop()
        {
            CurrentRunner?.Stop();
        }

        public int Spawn()
        {
            return CurrentRunner != null ? CurrentRunner.Spawn() : -1;
        }

        public int Spawn(CountdownSpec spec)
        {
            return CurrentRunner != null ? CurrentRunner.Spawn(spec) : -1;
        }

        public bool Cancel(int id)
        {
            return CurrentRunner != null && CurrentRunner.Cancel(id);
        }

        public int GetSuccessCount()
        {
            return CurrentRunner?.GetSuccessCount() ?? 0;
        }

        [ContextMenu("Spawn Test Countdown")]
        private void SpawnTestCountdown()
        {
            if (IsPlaying)
            {
                Spawn();
            }
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _poller.Poll(OnPressed, null);
            CurrentRunner.Tick(Time.deltaTime);
        }

        private void OnPressed(InputToken token)
        {
            CurrentRunner.Press(token);
        }

        private void HandleCompleted(CountdownStackReport report)
        {
            if (CurrentRunner != null)
            {
                CurrentRunner.Completed -= HandleCompleted;
            }

            inputMap?.DisableAll();

            _onCompleted?.Invoke(report);
            Completed?.Invoke(report);
        }
    }
}
