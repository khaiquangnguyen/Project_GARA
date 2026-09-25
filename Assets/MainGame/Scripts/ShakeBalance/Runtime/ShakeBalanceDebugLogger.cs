using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// Logs a one-line summary whenever the attached <see cref="ShakeBalancePlayer"/>
    /// finishes a run.
    /// </summary>
    public class ShakeBalanceDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private ShakeBalancePlayer player;

        private void OnEnable()
        {
            if (player != null)
            {
                player.BalanceCompleted += HandleBalanceCompleted;
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.BalanceCompleted -= HandleBalanceCompleted;
            }
        }

        private void HandleBalanceCompleted(ShakeBalanceReport report)
        {
            Debug.Log($"[ShakeBalance] {Describe(report)}");
        }

        /// <summary>One-line summary shared by every ShakeBalance log.</summary>
        public static string Describe(ShakeBalanceReport report)
        {
            return $"result={report.Result:P0} (unpenalized {report.UnpenalizedResult:P0}), end={report.EndReason}, " +
                   $"avgQuality={report.AverageQuality:P0}, perfect={report.PerfectTime:F1}s good={report.GoodTime:F1}s off={report.OffTime:F1}s, " +
                   $"pushes={report.Pushes}, instability={report.FinalInstability:F2}, elapsed={report.Elapsed:F2}s";
        }
    }
}
