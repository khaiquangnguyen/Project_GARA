using System;
using GARA.Input;

namespace GARA.InputCombos
{
    /// <summary>
    /// Drives a class-agnostic, session-long ordered input combo. Plain C# — host it from a
    /// MonoBehaviour driver (see <see cref="InputComboPlayer"/>).
    /// </summary>
    public sealed class InputComboRunner
    {
        private float _comboStartTime;
        private float _lastStepTime;
        private int _attemptsStarted;
        private int _resets;
        private int _resetsWrongInput;
        private int _resetsTimedOut;
        private int _stepsAccepted;
        private int _bestStepsReached;
        private int _idlePresses;

        public InputComboRunner(InputComboDefinition definition)
        {
            Definition = definition;
        }

        public bool IsRunning { get; private set; }
        public float ElapsedTime { get; private set; }
        public InputComboDefinition Definition { get; }
        public bool ComboInProgress => CurrentStep > 0 && !IsDeadlinePassed();
        public int CurrentStep { get; private set; }
        public InputToken ExpectedInput => Definition.steps[CurrentStep].input;

        public float TimeRemaining
        {
            get
            {
                if (CurrentStep == 0)
                {
                    return float.PositiveInfinity;
                }

                if (Definition.timingMode == ComboTimingMode.FromFirstInput)
                {
                    return Definition.comboTimeAllowed - (ElapsedTime - _comboStartTime);
                }

                return Definition.steps[CurrentStep].timeAllowed - (ElapsedTime - _lastStepTime);
            }
        }

        public float RunningScore { get; private set; }
        public int CompletedCombos { get; private set; }

        public event Action ComboStarted;
        public event Action<ComboStepAccepted> StepAccepted;
        public event Action<ComboStepAccepted> ComboCompleted;
        public event Action<ComboReset> ComboReset;
        public event Action<InputComboReport> Completed;

        public float GetRunningScore()
        {
            return RunningScore;
        }

        public InputComboReport Snapshot()
        {
            return BuildReport(false);
        }

        public void Start()
        {
            ElapsedTime = 0f;
            CurrentStep = 0;
            _comboStartTime = 0f;
            _lastStepTime = 0f;
            RunningScore = 0f;
            CompletedCombos = 0;
            _attemptsStarted = 0;
            _resets = 0;
            _resetsWrongInput = 0;
            _resetsTimedOut = 0;
            _stepsAccepted = 0;
            _bestStepsReached = 0;
            _idlePresses = 0;
            IsRunning = true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsRunning)
            {
                return;
            }

            ElapsedTime += deltaSeconds;

            if (CurrentStep > 0 && IsDeadlinePassed())
            {
                RaiseReset(ComboResetReason.TimedOut, CurrentStep, null);
                GoIdle();
            }

            if (Definition.sessionDuration > 0f && ElapsedTime >= Definition.sessionDuration)
            {
                var report = BuildReport(false);
                IsRunning = false;
                Completed?.Invoke(report);
            }
        }

        public void Press(InputToken token)
        {
            if (!IsRunning)
            {
                return;
            }

            if (CurrentStep == 0)
            {
                HandleIdlePress(token);
                return;
            }

            if (IsDeadlinePassed())
            {
                RaiseReset(ComboResetReason.TimedOut, CurrentStep, null);
                GoIdle();
                HandleIdlePress(token);
                return;
            }

            if (token == Definition.steps[CurrentStep].input)
            {
                AcceptCurrentStep();
                return;
            }

            RaiseReset(ComboResetReason.WrongInput, CurrentStep, token);
            GoIdle();

            if (Definition.restartOnResetInput && token == Definition.steps[0].input)
            {
                HandleIdlePress(token);
            }
        }

        public void ResetScore()
        {
            RunningScore = 0f;
            CompletedCombos = 0;
            _attemptsStarted = 0;
            _resets = 0;
            _resetsWrongInput = 0;
            _resetsTimedOut = 0;
            _stepsAccepted = 0;
            _bestStepsReached = 0;
            _idlePresses = 0;
        }

        public void Stop()
        {
            if (CurrentStep > 0)
            {
                RaiseReset(ComboResetReason.Stopped, CurrentStep, null);
                GoIdle();
            }

            var report = BuildReport(true);
            IsRunning = false;
            Completed?.Invoke(report);
        }

        private void HandleIdlePress(InputToken token)
        {
            if (Definition.steps.Length == 0)
            {
                return;
            }

            if (token == Definition.steps[0].input)
            {
                AcceptCurrentStep();
            }
            else
            {
                _idlePresses++;
            }
        }

        private void AcceptCurrentStep()
        {
            var isFirst = CurrentStep == 0;

            if (isFirst)
            {
                _comboStartTime = ElapsedTime;
                _attemptsStarted++;
                ComboStarted?.Invoke();
            }

            var timeSinceComboStart = ElapsedTime - _comboStartTime;
            var timeSincePreviousStep = isFirst ? 0f : ElapsedTime - _lastStepTime;
            _lastStepTime = ElapsedTime;

            var step = Definition.steps[CurrentStep];
            RunningScore += step.scoreValue;
            _stepsAccepted++;

            var accepted = new ComboStepAccepted(CurrentStep, step, timeSinceComboStart, timeSincePreviousStep, RunningScore);
            StepAccepted?.Invoke(accepted);

            var isLast = CurrentStep == Definition.steps.Length - 1;
            if (isLast)
            {
                RunningScore += Definition.completionBonus;
                CompletedCombos++;
                ComboCompleted?.Invoke(accepted);
                CurrentStep = 0;
            }
            else
            {
                CurrentStep++;
            }
        }

        private void GoIdle()
        {
            CurrentStep = 0;
        }

        private void RaiseReset(ComboResetReason reason, int stepsReached, InputToken? received)
        {
            _resets++;

            if (reason == ComboResetReason.WrongInput)
            {
                _resetsWrongInput++;
            }
            else if (reason == ComboResetReason.TimedOut)
            {
                _resetsTimedOut++;
            }

            if (stepsReached > _bestStepsReached)
            {
                _bestStepsReached = stepsReached;
            }

            var timeSinceComboStart = ElapsedTime - _comboStartTime;
            ComboReset?.Invoke(new GARA.InputCombos.ComboReset(reason, stepsReached, received, timeSinceComboStart));
        }

        private bool IsDeadlinePassed()
        {
            if (CurrentStep == 0)
            {
                return false;
            }

            if (Definition.timingMode == ComboTimingMode.FromFirstInput)
            {
                return ElapsedTime - _comboStartTime > Definition.comboTimeAllowed;
            }

            return ElapsedTime - _lastStepTime > Definition.steps[CurrentStep].timeAllowed;
        }

        private InputComboReport BuildReport(bool wasStopped)
        {
            return new InputComboReport
            {
                TotalScore = RunningScore,
                CompletedCombos = CompletedCombos,
                AttemptsStarted = _attemptsStarted,
                Resets = _resets,
                ResetsWrongInput = _resetsWrongInput,
                ResetsTimedOut = _resetsTimedOut,
                StepsAccepted = _stepsAccepted,
                BestStepsReached = _bestStepsReached,
                IdlePresses = _idlePresses,
                Elapsed = ElapsedTime,
                WasStopped = wasStopped,
                CompletionRate = _attemptsStarted > 0 ? (float)CompletedCombos / _attemptsStarted : 0f
            };
        }
    }
}
