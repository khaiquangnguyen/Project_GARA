using System;
using System.Collections.Generic;
using System.Linq;
using GARA.Input;
using GARA.InputSets;
using UnityEngine;

namespace GARA.Chords
{
    /// <summary>
    /// Drives a class-agnostic chord minigame: hold down every required token together,
    /// within a simultaneity tolerance, order irrelevant; too spread out or an extra key
    /// resets the chord.
    /// </summary>
    public sealed class ChordSequenceRunner
    {
        private readonly ChordSequenceDefinition _definition;
        private readonly ChordDefinition[] _chords;
        private readonly InputSetResult[] _results;
        private readonly Dictionary<InputToken, float> _held = new Dictionary<InputToken, float>();

        private bool _isRunning;
        private float _elapsedTime;
        private int _currentSetIndex;
        private int _currentAttempt;
        private int _bestProgressThisSet;
        private float _setEnterElapsed;
        private float _attemptStartElapsed;
        private float _timerBaseElapsed;
        private bool _waitingForRelease;
        private int _pendingRetryAttempt;
        private bool _completedRaised;

        public event Action<int, int> SetStarted;
        public event Action<ChordFailure> SetFailed;
        public event Action<InputSetResult> SetFinished;
        public event Action<InputSetCompletionReport> Completed;

        public ChordSequenceRunner(ChordSequenceDefinition definition)
        {
            _definition = definition;
            _chords = definition != null ? definition.Chords : Array.Empty<ChordDefinition>();
            _results = new InputSetResult[_chords.Length];
        }

        public bool IsRunning => _isRunning;

        public float ElapsedTime => _elapsedTime;

        public ChordSequenceDefinition Definition => _definition;

        public int CurrentSetIndex => _currentSetIndex;

        public int CurrentAttempt => _currentAttempt;

        public float CurrentSetTimeRemaining
        {
            get
            {
                if (!_isRunning || _currentSetIndex >= _chords.Length)
                {
                    return float.PositiveInfinity;
                }

                var timeLimit = _chords[_currentSetIndex].timeLimit;
                if (timeLimit <= 0f)
                {
                    return float.PositiveInfinity;
                }

                return Mathf.Max(0f, timeLimit - (_elapsedTime - _timerBaseElapsed));
            }
        }

        public IReadOnlyList<InputToken> RequiredNow
        {
            get
            {
                if (!_isRunning || _currentSetIndex >= _chords.Length)
                {
                    return Array.Empty<InputToken>();
                }

                return _chords[_currentSetIndex].required ?? Array.Empty<InputToken>();
            }
        }

        public IReadOnlyList<InputToken> HeldNow => _held.Keys.ToList();

        public bool WaitingForRelease => _waitingForRelease;

        public void Start()
        {
            _isRunning = true;
            _elapsedTime = 0f;
            _completedRaised = false;
            _waitingForRelease = false;
            _held.Clear();
            Array.Clear(_results, 0, _results.Length);

            if (_chords.Length == 0)
            {
                FinishSequence(false, false);
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
                TimeOutSequence();
                return;
            }

            if (_currentSetIndex >= _chords.Length || _waitingForRelease)
            {
                return;
            }

            var timeLimit = _chords[_currentSetIndex].timeLimit;
            if (timeLimit > 0f && CurrentSetTimeRemaining <= 0f)
            {
                FailChord(InputSetFailureReason.SetTimedOut, 0f);
            }
        }

        public void Press(InputToken token)
        {
            if (!_isRunning || _currentSetIndex >= _chords.Length)
            {
                return;
            }

            _held[token] = _elapsedTime;

            if (_waitingForRelease)
            {
                return;
            }

            var chord = _chords[_currentSetIndex];
            var required = chord.required ?? Array.Empty<InputToken>();

            if (Contains(required, token))
            {
                var heldRequiredCount = CountHeld(required);
                _bestProgressThisSet = Math.Max(_bestProgressThisSet, heldRequiredCount);

                if (heldRequiredCount >= required.Length)
                {
                    var spread = ComputeSpread(required);
                    var window = _definition.WindowFor(_currentSetIndex);

                    if (spread <= window)
                    {
                        ClearCurrentChord(spread);
                    }
                    else
                    {
                        FailChord(InputSetFailureReason.ChordSpread, spread);
                    }
                }

                return;
            }

            if (_definition.ExtraInputFailsChord)
            {
                FailChord(InputSetFailureReason.ExtraInput, 0f);
            }
        }

        public void Release(InputToken token)
        {
            if (!_isRunning)
            {
                return;
            }

            _held.Remove(token);

            if (_waitingForRelease && _held.Count == 0)
            {
                _waitingForRelease = false;
                RetrySet(_currentSetIndex, _pendingRetryAttempt);
            }
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

            _waitingForRelease = false;
            FinishSequence(true, false);
        }

        private static bool Contains(InputToken[] tokens, InputToken token)
        {
            foreach (var candidate in tokens)
            {
                if (candidate.Equals(token))
                {
                    return true;
                }
            }

            return false;
        }

        private int CountHeld(InputToken[] required)
        {
            var count = 0;
            foreach (var token in required)
            {
                if (_held.ContainsKey(token))
                {
                    count++;
                }
            }

            return count;
        }

        private float ComputeSpread(InputToken[] required)
        {
            var min = float.PositiveInfinity;
            var max = float.NegativeInfinity;

            foreach (var token in required)
            {
                var pressTime = _held[token];
                if (pressTime < min)
                {
                    min = pressTime;
                }

                if (pressTime > max)
                {
                    max = pressTime;
                }
            }

            return max - min;
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
            _currentAttempt = attempt;
            _attemptStartElapsed = _elapsedTime;

            if (resetSetTimer)
            {
                _timerBaseElapsed = _elapsedTime;
            }

            SetStarted?.Invoke(setIndex, attempt);
        }

        private void ClearCurrentChord(float spread)
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
            _held.Clear();
            SetFinished?.Invoke(result);

            AdvanceAfterSetFinished();
        }

        private void AdvanceAfterSetFinished()
        {
            var nextIndex = _currentSetIndex + 1;
            if (nextIndex >= _chords.Length)
            {
                FinishSequence(false, false);
                return;
            }

            BeginNewSet(nextIndex);
        }

        private void FailChord(InputSetFailureReason reason, float spread)
        {
            var heldSnapshot = _held.Keys.ToList();
            var failure = new ChordFailure(_currentSetIndex, _currentAttempt, reason, spread, heldSnapshot);
            SetFailed?.Invoke(failure);

            var maxAttempts = _definition.RetryPolicy.maxAttemptsPerSet;
            var nextAttempt = _currentAttempt + 1;

            if (maxAttempts > 0 && nextAttempt > maxAttempts)
            {
                ExhaustSet();
                return;
            }

            if (_definition.RequireAllReleasedBeforeRetry)
            {
                _waitingForRelease = true;
                _pendingRetryAttempt = nextAttempt;

                if (_held.Count == 0)
                {
                    _waitingForRelease = false;
                    RetrySet(_currentSetIndex, nextAttempt);
                }
            }
            else
            {
                RetrySet(_currentSetIndex, nextAttempt);
            }
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
                FinishSequence(false, false);
                return;
            }

            AdvanceAfterSetFinished();
        }

        private void TimeOutSequence()
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

            FinishSequence(false, true);
        }

        private void FinishSequence(bool wasAborted, bool sequenceTimedOut)
        {
            if (_completedRaised)
            {
                return;
            }

            _isRunning = false;
            _completedRaised = true;

            var report = BuildReport(wasAborted, sequenceTimedOut);
            Completed?.Invoke(report);
        }

        private InputSetCompletionReport BuildReport(bool wasAborted, bool sequenceTimedOut)
        {
            var totalSets = _chords.Length;
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
                SetCollectionTimedOut = sequenceTimedOut,
                SetResults = (InputSetResult[])_results.Clone()
            };
        }
    }
}
