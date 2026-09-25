using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>Scratch-scene aid: logs a one-line summary whenever a rhythm sequence finishes.</summary>
    public class RhythmDebugLogger : MonoBehaviour
    {
        [SerializeField]
        private RhythmSequencePlayer player;

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

        private void HandleSequenceCompleted(RhythmCompletionReport report)
        {
            Debug.Log($"[Rhythm] {report.HitNotes}/{report.TotalNotes} hit " +
                      $"(P={report.PerfectCount} G={report.GoodCount} O={report.OkCount} M={report.MissCount}), " +
                      $"accuracy={report.AccuracyScore:F2}, stray={report.StrayPresses}, aborted={report.WasAborted}");
        }
    }
}
