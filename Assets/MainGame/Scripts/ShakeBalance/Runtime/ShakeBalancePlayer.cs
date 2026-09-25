using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// MonoBehaviour driver that polls an <see cref="InputTokenMap"/> and feeds presses into
    /// a <see cref="ShakeBalanceRunner"/>.
    /// </summary>
    public class ShakeBalancePlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private ShakeBalanceRunner _runner;
        private Action<InputToken> _onTokenPressed;
        private Action<ShakeBalanceReport> _onCompleted;

        public event Action<ShakeBalanceReport> BalanceCompleted;

        /// <summary>Raised by any player when it starts a run, so scene-level visuals can present it without referencing the character.</summary>
        public static event Action<ShakeBalanceRunner> AnyBalanceStarted;

        /// <summary>Raised by any player once its run ends, however it ended.</summary>
        public static event Action<ShakeBalanceRunner> AnyBalanceEnded;

        public bool IsPlaying => _runner != null && _runner.IsRunning;

        public ShakeBalanceRunner CurrentRunner => _runner;

        public void Play(ShakeBalanceDefinition definition, Action<ShakeBalanceReport> onCompleted = null)
        {
            Abort();

            _poller = new InputTokenPoller(inputMap);
            _runner = new ShakeBalanceRunner(definition);
            _onCompleted = onCompleted;

            inputMap?.EnableAll();

            // Raised before Start() so visuals are bound for the whole run, and before subscribing
            // to Completed so visuals see the end (and play its feedbacks) before AnyBalanceEnded hides them.
            AnyBalanceStarted?.Invoke(_runner);
            _runner.Completed += HandleCompleted;
            _runner.Start();
        }

        /// <summary>Ends the run as a cash out — keeps the full result.</summary>
        public void Stop()
        {
            _runner?.Stop();
        }

        /// <summary>Cuts the run short; onCompleted still fires, with an Aborted report.</summary>
        public void Abort()
        {
            if (_runner == null)
            {
                return;
            }

            _runner.Abort();
            _runner.Completed -= HandleCompleted;
            _runner = null;
            _onCompleted = null;
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

            _runner.Tick(Time.deltaTime);

            if (!IsPlaying)
            {
                return;
            }

            _poller.Poll(_onTokenPressed, null);
        }

        private void OnDisable()
        {
            Abort();
        }

        private void HandleTokenPressed(InputToken token)
        {
            _runner?.Press(token);
        }

        private void HandleCompleted(ShakeBalanceReport report)
        {
            AnyBalanceEnded?.Invoke(_runner);
            _onCompleted?.Invoke(report);
            BalanceCompleted?.Invoke(report);
        }
    }
}
