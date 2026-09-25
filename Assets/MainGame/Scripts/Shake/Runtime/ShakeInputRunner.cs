using System;
using GARA.Input;
using UnityEngine;

namespace GARA.Shake
{
    /// <summary>
    /// Drives a <see cref="ShakeInputDefinition"/>: counts alternating A/B presses as completed
    /// pairs. Plain C# so it can be unit tested and driven by any host.
    /// </summary>
    public sealed class ShakeInputRunner
    {
        private readonly ShakeInputDefinition _definition;

        private InputToken? _lastAccepted;
        private bool _hasStarted;
        private int _completedPairs;
        private int _acceptedPresses;
        private int _wrongPresses;
        private int _currentStreak;
        private int _bestStreak;
        private float _elapsedTime;
        private bool _isRunning;
        private bool _completed;

        public ShakeInputRunner(ShakeInputDefinition definition)
        {
            _definition = definition;
        }

        public bool IsRunning => _isRunning;
        public float ElapsedTime => _elapsedTime;

        public float TimeRemaining => _definition.Duration <= 0f
            ? float.PositiveInfinity
            : Mathf.Max(0f, _definition.Duration - _elapsedTime);

        public int CompletedPairs => _completedPairs;
        public int CurrentStreak => _currentStreak;
        public int BestStreak => _bestStreak;
        public int WrongPresses => _wrongPresses;

        /// <summary>Null before the first accepted press, unless RequireStartWithFirst forces `first`.</summary>
        public InputToken? ExpectedNext
        {
            get
            {
                if (!_hasStarted)
                {
                    return _definition.RequireStartWithFirst ? (InputToken?)_definition.First : null;
                }

                return _lastAccepted.Value == _definition.First ? _definition.Second : _definition.First;
            }
        }

        public event Action<int> PairCompleted;
        public event Action<InputToken> PressAccepted;
        public event Action<InputToken> PressRejected;
        public event Action<ShakeInputReport> Completed;

        public int GetCompletedPairCount()
        {
            return _completedPairs;
        }

        public void Start()
        {
            _lastAccepted = null;
            _hasStarted = false;
            _completedPairs = 0;
            _acceptedPresses = 0;
            _wrongPresses = 0;
            _currentStreak = 0;
            _bestStreak = 0;
            _elapsedTime = 0f;
            _isRunning = true;
            _completed = false;
        }

        public void Tick(float deltaSeconds)
        {
            if (!_isRunning || _completed)
            {
                return;
            }

            _elapsedTime += deltaSeconds;

            if (_definition.Duration > 0f && _elapsedTime >= _definition.Duration)
            {
                CompleteInternal(false);
            }
        }

        public void Press(InputToken token)
        {
            if (!_isRunning || _completed)
            {
                return;
            }

            var isFirst = token == _definition.First;
            var isSecond = token == _definition.Second;
            if (!isFirst && !isSecond)
            {
                return;
            }

            if (!_hasStarted)
            {
                if (_definition.RequireStartWithFirst && isSecond)
                {
                    _wrongPresses++;
                    PressRejected?.Invoke(token);
                    return;
                }

                AcceptPress(token);
                return;
            }

            if (token == _lastAccepted.Value)
            {
                _wrongPresses++;
                if (_definition.WrongPressBreaksStreak)
                {
                    _currentStreak = 0;
                }

                PressRejected?.Invoke(token);
                return;
            }

            AcceptPress(token);
        }

        public void Stop()
        {
            if (!_isRunning || _completed)
            {
                return;
            }

            CompleteInternal(true);
        }

        public ShakeInputReport Snapshot()
        {
            return BuildReport(false);
        }

        private void AcceptPress(InputToken token)
        {
            _hasStarted = true;
            _lastAccepted = token;
            _acceptedPresses++;
            _currentStreak++;
            if (_currentStreak > _bestStreak)
            {
                _bestStreak = _currentStreak;
            }

            PressAccepted?.Invoke(token);

            if (_acceptedPresses % 2 == 0)
            {
                _completedPairs++;
                PairCompleted?.Invoke(_completedPairs);
            }
        }

        private ShakeInputReport BuildReport(bool wasStoppedExternally)
        {
            return new ShakeInputReport(_completedPairs, _acceptedPresses, _wrongPresses, _bestStreak, _elapsedTime, wasStoppedExternally);
        }

        private void CompleteInternal(bool wasStoppedExternally)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _isRunning = false;

            var report = BuildReport(wasStoppedExternally);
            Completed?.Invoke(report);
        }
    }
}
