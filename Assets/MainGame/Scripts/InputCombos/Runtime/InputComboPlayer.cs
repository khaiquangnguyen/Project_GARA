using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.InputCombos
{
    /// <summary>
    /// MonoBehaviour driver that polls an <see cref="InputTokenMap"/> and ticks an
    /// <see cref="InputComboRunner"/> each frame.
    /// </summary>
    public class InputComboPlayer : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private Action<InputComboReport> _onCompleted;

        public bool IsPlaying => CurrentRunner != null && CurrentRunner.IsRunning;
        public InputComboRunner CurrentRunner { get; private set; }

        public float RunningScore => CurrentRunner?.RunningScore ?? 0f;
        public int CompletedCombos => CurrentRunner?.CompletedCombos ?? 0;
        public int CurrentStep => CurrentRunner?.CurrentStep ?? 0;
        public float TimeRemaining => CurrentRunner?.TimeRemaining ?? float.PositiveInfinity;

        public event Action<ComboStepAccepted> StepAccepted;
        public event Action<ComboStepAccepted> ComboCompleted;
        public event Action<InputComboReport> Completed;

        public void Play(InputComboDefinition definition, Action<InputComboReport> onCompleted = null)
        {
            _poller ??= new InputTokenPoller(inputMap);

            CurrentRunner = new InputComboRunner(definition);
            _onCompleted = onCompleted;
            CurrentRunner.StepAccepted += HandleStepAccepted;
            CurrentRunner.ComboCompleted += HandleComboCompleted;
            CurrentRunner.Completed += HandleCompleted;
            CurrentRunner.Start();

            inputMap?.EnableAll();
        }

        public void Stop()
        {
            CurrentRunner?.Stop();
        }

        public void ResetScore()
        {
            CurrentRunner?.ResetScore();
        }

        public float GetRunningScore()
        {
            return CurrentRunner?.GetRunningScore() ?? 0f;
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

        private void HandleStepAccepted(ComboStepAccepted accepted)
        {
            StepAccepted?.Invoke(accepted);
        }

        private void HandleComboCompleted(ComboStepAccepted accepted)
        {
            ComboCompleted?.Invoke(accepted);
        }

        private void HandleCompleted(InputComboReport report)
        {
            if (CurrentRunner != null)
            {
                CurrentRunner.StepAccepted -= HandleStepAccepted;
                CurrentRunner.ComboCompleted -= HandleComboCompleted;
                CurrentRunner.Completed -= HandleCompleted;
            }

            inputMap?.DisableAll();

            _onCompleted?.Invoke(report);
            Completed?.Invoke(report);
        }
    }
}
