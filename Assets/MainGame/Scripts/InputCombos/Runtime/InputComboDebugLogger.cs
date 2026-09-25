using UnityEngine;

namespace GARA.InputCombos
{
    /// <summary>
    /// Scratch-scene helper that logs an <see cref="InputComboPlayer"/>'s events to the console.
    /// </summary>
    public class InputComboDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private InputComboPlayer player;

        private InputComboRunner _subscribedRunner;

        private void Update()
        {
            if (player == null || player.CurrentRunner == _subscribedRunner)
            {
                return;
            }

            Unsubscribe();
            _subscribedRunner = player.CurrentRunner;
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribedRunner == null)
            {
                return;
            }

            _subscribedRunner.ComboStarted += HandleComboStarted;
            _subscribedRunner.StepAccepted += HandleStepAccepted;
            _subscribedRunner.ComboCompleted += HandleComboCompleted;
            _subscribedRunner.ComboReset += HandleComboReset;
            _subscribedRunner.Completed += HandleCompleted;
        }

        private void Unsubscribe()
        {
            if (_subscribedRunner == null)
            {
                return;
            }

            _subscribedRunner.ComboStarted -= HandleComboStarted;
            _subscribedRunner.StepAccepted -= HandleStepAccepted;
            _subscribedRunner.ComboCompleted -= HandleComboCompleted;
            _subscribedRunner.ComboReset -= HandleComboReset;
            _subscribedRunner.Completed -= HandleCompleted;
        }

        private void HandleComboStarted()
        {
            Debug.Log("[InputCombos] Combo started");
        }

        private void HandleStepAccepted(ComboStepAccepted accepted)
        {
            Debug.Log($"[InputCombos] Step {accepted.StepIndex} accepted, score={accepted.ScoreAfter:F1}");
        }

        private void HandleComboCompleted(ComboStepAccepted accepted)
        {
            Debug.Log($"[InputCombos] Combo completed at step {accepted.StepIndex}");
        }

        private void HandleComboReset(ComboReset reset)
        {
            Debug.Log($"[InputCombos] Reset reason={reset.Reason} stepsReached={reset.StepsReached}");
        }

        private void HandleCompleted(InputComboReport report)
        {
            Debug.Log($"[InputCombos] Session complete: score={report.TotalScore:F1}, completed={report.CompletedCombos}/{report.AttemptsStarted}, stopped={report.WasStopped}");
        }
    }
}
