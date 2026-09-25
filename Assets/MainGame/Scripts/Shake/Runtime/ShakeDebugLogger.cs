using UnityEngine;

namespace GARA.Shake
{
    /// <summary>Scratch-scene aid: logs a one-line summary whenever a shake-input run finishes.</summary>
    public class ShakeDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private ShakeInputPlayer player;

        private void OnEnable()
        {
            if (player != null)
            {
                player.ShakeCompleted += HandleCompleted;
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.ShakeCompleted -= HandleCompleted;
            }
        }

        private void HandleCompleted(ShakeInputReport report)
        {
            Debug.Log($"[Shake] pairs={report.CompletedPairs}, wrong={report.WrongPresses}, " +
                      $"bestStreak={report.BestStreak}, elapsed={report.Elapsed:F2}s, " +
                      $"pps={report.PairsPerSecond:F2}, stoppedExternally={report.WasStoppedExternally}");
        }
    }
}
