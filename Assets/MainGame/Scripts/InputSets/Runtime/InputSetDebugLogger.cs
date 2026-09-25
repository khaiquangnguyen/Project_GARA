using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// Logs a one-line summary whenever the attached <see cref="InputSetCollectionPlayer"/>
    /// finishes a run.
    /// </summary>
    public class InputSetDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private InputSetCollectionPlayer player;

        private void OnEnable()
        {
            if (player != null)
            {
                player.SetCollectionCompleted += HandleSetCollectionCompleted;
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.SetCollectionCompleted -= HandleSetCollectionCompleted;
            }
        }

        private void HandleSetCollectionCompleted(InputSetCompletionReport report)
        {
            Debug.Log(
                $"[InputSets] cleared {report.ClearedSets}/{report.TotalSets} sets " +
                $"({report.CompletionRate:P0}), firstTry={report.FirstTryRate:P0}, " +
                $"attempts={report.TotalAttempts}, aborted={report.WasAborted}, " +
                $"setCollectionTimedOut={report.SetCollectionTimedOut}, elapsed={report.TotalElapsed:F2}s");
        }
    }
}
