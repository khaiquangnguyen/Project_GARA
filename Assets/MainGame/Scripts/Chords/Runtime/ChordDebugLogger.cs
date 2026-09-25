using GARA.InputSets;
using UnityEngine;

namespace GARA.Chords
{
    /// <summary>
    /// Logs a one-line summary whenever the attached <see cref="ChordSequencePlayer"/>
    /// finishes a run.
    /// </summary>
    public class ChordDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private ChordSequencePlayer player;

        private void OnEnable()
        {
            if (player != null)
            {
                player.SequenceCompleted += HandleSequenceCompleted;
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.SequenceCompleted -= HandleSequenceCompleted;
            }
        }

        private void HandleSequenceCompleted(InputSetCompletionReport report)
        {
            Debug.Log(
                $"[Chords] cleared {report.ClearedSets}/{report.TotalSets} chords " +
                $"({report.CompletionRate:P0}), firstTry={report.FirstTryRate:P0}, " +
                $"attempts={report.TotalAttempts}, aborted={report.WasAborted}, " +
                $"sequenceTimedOut={report.SetCollectionTimedOut}, elapsed={report.TotalElapsed:F2}s");
        }
    }
}
