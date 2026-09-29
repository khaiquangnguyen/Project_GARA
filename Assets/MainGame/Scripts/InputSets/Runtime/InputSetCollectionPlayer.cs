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
    public class InputSetCollectionPlayer : MonoBehaviour, IAutoPlayable
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

        // Dev-only: plays the next collection by itself instead of reading
        // input; back to Off once that collection ends.
        public AutoPlayMode AutoPlay { get; set; }

        private const float AutoPressInterval = 0.12f;
        private const int AutoWrongAttempts = 2;

        private bool[] _autoFailSets;
        private float _autoPressTimer;
        public void Play(InputSetCollectionDefinition definition, Action<InputSetCompletionReport> onCompleted = null)
        {
            Abort();

            _poller = new InputTokenPoller(inputMap);
            _runner = new InputSetCollectionRunner(definition);
            _onCompleted = onCompleted;
            _runner.Completed += HandleCompleted;

            inputMap?.EnableAll();
            PrepareAutoPlay(definition);

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

            if (AutoPlay == AutoPlayMode.Off)
            {
                _poller.Poll(HandlePressed, null);
                return;
            }

            _autoPressTimer -= Time.deltaTime;
            if (_autoPressTimer <= 0f)
            {
                _autoPressTimer = AutoPressInterval;
                AutoPress();
            }
        }

        // RandomFail marks some sets (at least one) to be fumbled.
        private void PrepareAutoPlay(InputSetCollectionDefinition definition)
        {
            var setCount = definition != null ? definition.Sets.Length : 0;
            _autoFailSets = new bool[setCount];
            _autoPressTimer = AutoPressInterval;
            if (AutoPlay != AutoPlayMode.RandomFail || setCount == 0)
            {
                return;
            }

            for (var i = 0; i < setCount; i++)
            {
                _autoFailSets[i] = UnityEngine.Random.value < 0.35f;
            }

            _autoFailSets[UnityEngine.Random.Range(0, setCount)] = true;
        }

        // The expected input, or a wrong one on a fumbled set's first
        // attempts (it clears after that if retries never run out).
        private void AutoPress()
        {
            if (!(_runner.ExpectedInput is InputToken expected))
            {
                return;
            }

            var fumble = _runner.CurrentSetIndex < _autoFailSets.Length
                         && _autoFailSets[_runner.CurrentSetIndex]
                         && _runner.CurrentAttempt <= AutoWrongAttempts;
            _runner.Press(fumble ? WrongInput(expected) : expected);
        }

        private InputToken WrongInput(InputToken expected)
        {
            if (inputMap != null)
            {
                foreach (var entry in inputMap.Entries)
                {
                    if (entry.token != expected)
                    {
                        return entry.token;
                    }
                }
            }

            return new InputToken(expected.Id + 1);
        }

        private void HandlePressed(InputToken token)
        {
            _runner?.Press(token);
        }

        private void HandleCompleted(InputSetCompletionReport report)
        {
            AutoPlay = AutoPlayMode.Off;
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
