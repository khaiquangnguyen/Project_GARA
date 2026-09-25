using System;
using GARA.Input;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// Drives a class-agnostic ordered-input-set minigame: press the right token, in order,
    /// within each set's time limit; a wrong press or timeout resets only that set.
    /// </summary>
    public sealed class InputSetCollectionRunner
    {
        private readonly InputSetCollectionDefinition _definition;
        private readonly InputSetDefinition[] _sets;
        private readonly InputSetResult[] _results;

        private bool _isRunning;
        private float _elapsedTime;
        private int _currentSetIndex;
        private int _currentInputIndex;
        private int _currentAttempt;
        private int _bestProgressThisSet;
        private float _setEnterElapsed;
        private float _attemptStartElapsed;
        private float _timerBaseElapsed;
        private bool _completedRaised;
        private bool _timedOut;

        public event Action<int, int> SetStarted;
        public event Action<int, int> InputAccepted;
        public event Action<InputSetFailure> SetFailed;
        public event Action<InputSetResult> SetFinished;
        public event Action<InputSetCompletionReport> Completed;

        public InputSetCollectionRunner(InputSetCollectionDefinition definition)
        {
            _definition = definition;
            _sets = definition != null ? definition.Sets : Array.Empty<InputSetDefinition>();
            _results = new InputSetResult[_sets.Length];
        }

        public bool IsRunning => _isRunning;

        public float ElapsedTime => _elapsedTime;

        public InputSetCollectionDefinition Definition => _definition;

        /// <summary>True once the set collection has ended because its total time limit ran out.</summary>
        public bool TimedOut => _timedOut;

        public int CurrentSetIndex => _currentSetIndex;

        public int CurrentInputIndex => _currentInputIndex;

        public int CurrentAttempt => _currentAttempt;

        public float CurrentSetTimeRemaining
        {
            get
            {
                if (!_isRunning || _currentSetIndex >= _sets.Length)
                {
                    return float.PositiveInfinity;
                }

                var timeLimit = _sets[_currentSetIndex].timeLimit;
                if (timeLimit <= 0f)
                {
                    return float.PositiveInfinity;
                }

                return Mathf.Max(0f, timeLimit - (_elapsedTime - _timerBaseElapsed));
            }
        }

        public InputToken? ExpectedInput
        {
            get
            {
                if (!_isRunning || _currentSetIndex >= _sets.Length)
                {
                    return null;
                }

                var inputs = _sets[_currentSetIndex].inputs;
                if (inputs == null || _currentInputIndex >= inputs.Length)
                {
                    return null;
                }

                return inputs[_currentInputIndex];
            }
        }

        public void Start()
        {
            _isRunning = true;
            _elapsedTime = 0f;
            _completedRaised = false;
            _timedOut = false;
            Array.Clear(_results, 0, _results.Length);

            if (_sets.Length == 0)
            {
                FinishSetCollection(false, false);
                return;
            }

            BeginNewSet(0);
        }

        public void Tick(float deltaSeconds)
        {
            if (!_isRunning)
            {
                return;
            }

            _elapsedTime += deltaSeconds;

            if (_definition.TotalTimeLimit > 0f && _elapsedTime >= _definition.TotalTimeLimit)
            {
                TimeOutSetCollection();
                return;
            }

            if (_currentSetIndex >= _sets.Length)
            {
                return;
            }

            var timeLimit = _sets[_currentSetIndex].timeLimit;
            if (timeLimit > 0f && CurrentSetTimeRemaining <= 0f)
            {
                var expected = ExpectedInput ?? default;
                FailAttempt(InputSetFailureReason.SetTimedOut, expected, default);
            }
        }

        public void Press(InputToken token)
        {
            if (!_isRunning || _currentSetIndex >= _sets.Length)
            {
                return;
            }

            var set = _sets[_currentSetIndex];
            var inputs = set.inputs ?? Array.Empty<InputToken>();

            if (_currentInputIndex < inputs.Length && inputs[_currentInputIndex].Equals(token))
            {
                _currentInputIndex++;
                _bestProgressThisSet = Math.Max(_bestProgressThisSet, _currentInputIndex);
                InputAccepted?.Invoke(_currentSetIndex, _currentInputIndex - 1);

                if (_currentInputIndex >= inputs.Length)
                {
                    ClearCurrentSet();
                }

                return;
            }

            var expectedToken = _currentInputIndex < inputs.Length ? inputs[_currentInputIndex] : default;
            FailAttempt(InputSetFailureReason.WrongInput, expectedToken, token);
        }

        public void Abort()
        {
            if (!_isRunning)
            {
                return;
            }

            for (var i = 0; i < _results.Length; i++)
            {
                if (_results[i].Outcome == InputSetOutcome.Pending)
                {
                    _results[i] = new InputSetResult(
                        i,
                        InputSetOutcome.Failed,
                        _results[i].Attempts,
                        false,
                        float.NaN,
                        _results[i].TimeInSet,
                        InputSetFailureReason.Aborted,
                        _results[i].BestProgress);
                }
            }

            FinishSetCollection(true, false);
        }

        private void BeginNewSet(int setIndex)
        {
            _setEnterElapsed = _elapsedTime;
            _bestProgressThisSet = 0;
            EnterSet(setIndex, 1, true);
        }

        private void RetrySet(int setIndex, int attempt)
        {
            EnterSet(setIndex, attempt, _definition.RetryPolicy.resetTimerOnRetry);
        }

        private void EnterSet(int setIndex, int attempt, bool resetSetTimer)
        {
            _currentSetIndex = setIndex;
            _currentInputIndex = 0;
            _currentAttempt = attempt;
            _attemptStartElapsed = _elapsedTime;

            if (resetSetTimer)
            {
                _timerBaseElapsed = _elapsedTime;
            }

            SetStarted?.Invoke(setIndex, attempt);
        }

        private void ClearCurrentSet()
        {
            var timeToClear = _elapsedTime - _attemptStartElapsed;
            var timeInSet = _elapsedTime - _setEnterElapsed;
            var result = new InputSetResult(
                _currentSetIndex,
                InputSetOutcome.Cleared,
                _currentAttempt,
                _currentAttempt == 1,
                timeToClear,
                timeInSet,
                InputSetFailureReason.None,
                _bestProgressThisSet);

            _results[_currentSetIndex] = result;
            SetFinished?.Invoke(result);

            AdvanceAfterSetFinished();
        }

        private void AdvanceAfterSetFinished()
        {
            var nextIndex = _currentSetIndex + 1;
            if (nextIndex >= _sets.Length)
            {
                FinishSetCollection(false, false);
                return;
            }

            BeginNewSet(nextIndex);
        }

        private void FailAttempt(InputSetFailureReason reason, InputToken expected, InputToken received)
        {
            var failure = new InputSetFailure(_currentSetIndex, _currentAttempt, _currentInputIndex, expected, received, reason);
            SetFailed?.Invoke(failure);

            var maxAttempts = _definition.RetryPolicy.maxAttemptsPerSet;
            var nextAttempt = _currentAttempt + 1;

            if (maxAttempts > 0 && nextAttempt > maxAttempts)
            {
                ExhaustSet();
                return;
            }

            RetrySet(_currentSetIndex, nextAttempt);
        }

        private void ExhaustSet()
        {
            var timeInSet = _elapsedTime - _setEnterElapsed;
            var result = new InputSetResult(
                _currentSetIndex,
                InputSetOutcome.Failed,
                _currentAttempt,
                false,
                float.NaN,
                timeInSet,
                InputSetFailureReason.AttemptsExhausted,
                _bestProgressThisSet);

            _results[_currentSetIndex] = result;
            SetFinished?.Invoke(result);

            if (_definition.RetryPolicy.onExhausted == ExhaustedSetBehaviour.EndSetCollection)
            {
                FinishSetCollection(false, false);
                return;
            }

            AdvanceAfterSetFinished();
        }

        private void TimeOutSetCollection()
        {
            for (var i = 0; i < _results.Length; i++)
            {
                if (_results[i].Outcome != InputSetOutcome.Cleared)
                {
                    var timeInSet = i == _currentSetIndex ? _elapsedTime - _setEnterElapsed : _results[i].TimeInSet;
                    _results[i] = new InputSetResult(
                        i,
                        InputSetOutcome.Failed,
                        _results[i].Attempts,
                        false,
                        float.NaN,
                        timeInSet,
                        InputSetFailureReason.SetCollectionTimedOut,
                        _results[i].BestProgress);
                }
            }

            FinishSetCollection(false, true);
        }

        private void FinishSetCollection(bool wasAborted, bool setCollectionTimedOut)
        {
            if (_completedRaised)
            {
                return;
            }

            _isRunning = false;
            _completedRaised = true;
            _timedOut = setCollectionTimedOut;

            var report = BuildReport(wasAborted, setCollectionTimedOut);
            Completed?.Invoke(report);
        }

        private InputSetCompletionReport BuildReport(bool wasAborted, bool setCollectionTimedOut)
        {
            var totalSets = _sets.Length;
            var clearedSets = 0;
            var firstTryClears = 0;
            var totalAttempts = 0;

            foreach (var result in _results)
            {
                if (result.Outcome == InputSetOutcome.Cleared)
                {
                    clearedSets++;
                    if (result.ClearedFirstTry)
                    {
                        firstTryClears++;
                    }
                }

                if (result.Outcome != InputSetOutcome.Pending)
                {
                    totalAttempts += result.Attempts;
                }
            }

            return new InputSetCompletionReport
            {
                TotalSets = totalSets,
                ClearedSets = clearedSets,
                FirstTryClears = firstTryClears,
                TotalAttempts = totalAttempts,
                CompletionRate = totalSets > 0 ? (float)clearedSets / totalSets : 0f,
                FirstTryRate = totalSets > 0 ? (float)firstTryClears / totalSets : 0f,
                AttemptEfficiency = totalAttempts > 0 ? (float)totalSets / totalAttempts : 0f,
                TotalElapsed = _elapsedTime,
                WasAborted = wasAborted,
                SetCollectionTimedOut = setCollectionTimedOut,
                SetResults = (InputSetResult[])_results.Clone()
            };
        }
    }
}
