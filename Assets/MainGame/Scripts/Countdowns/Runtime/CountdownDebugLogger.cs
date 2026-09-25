using UnityEngine;

namespace GARA.Countdowns
{
    /// <summary>
    /// Scratch-scene helper that logs a <see cref="CountdownStackPlayer"/>'s events to the console.
    /// </summary>
    public class CountdownDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private CountdownStackPlayer player;

        private CountdownStackRunner _subscribedRunner;

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

            _subscribedRunner.CountdownSpawned += HandleSpawned;
            _subscribedRunner.CountdownResolved += HandleResolved;
            _subscribedRunner.CountdownExpired += HandleExpired;
            _subscribedRunner.Whiffed += HandleWhiffed;
            _subscribedRunner.Completed += HandleCompleted;
        }

        private void Unsubscribe()
        {
            if (_subscribedRunner == null)
            {
                return;
            }

            _subscribedRunner.CountdownSpawned -= HandleSpawned;
            _subscribedRunner.CountdownResolved -= HandleResolved;
            _subscribedRunner.CountdownExpired -= HandleExpired;
            _subscribedRunner.Whiffed -= HandleWhiffed;
            _subscribedRunner.Completed -= HandleCompleted;
        }

        private void HandleSpawned(ActiveCountdown countdown)
        {
            Debug.Log($"[Countdowns] Spawned #{countdown.Id} duration={countdown.Duration:F2}");
        }

        private void HandleResolved(CountdownResult result)
        {
            Debug.Log($"[Countdowns] Resolved #{result.Id} outcome={result.Outcome} remaining={result.RemainingAtResolve:F2}");
        }

        private void HandleExpired(CountdownResult result)
        {
            Debug.Log($"[Countdowns] Missed #{result.Id}");
        }

        private void HandleWhiffed()
        {
            Debug.Log("[Countdowns] Whiff");
        }

        private void HandleCompleted(CountdownStackReport report)
        {
            Debug.Log($"[Countdowns] Completed: {report.SuccessCount}/{report.TotalCountdowns} success ({report.PerfectCount} perfect), whiffs={report.WhiffPresses}, stopped={report.WasStopped}");
        }
    }
}
