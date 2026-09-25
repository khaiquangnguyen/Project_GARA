using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>
    /// Play-mode test harness for a <see cref="RhythmVisualDriver"/>: generates a random
    /// sequence from <see cref="inputMap"/>'s tokens, runs and judges it against live input,
    /// and logs the completion report. Stands in for a character's <see cref="RhythmSequencePlayer"/>
    /// so the driver prefab can be tuned without a combat scene.
    /// </summary>
    [RequireComponent(typeof(RhythmVisualDriver))]
    public class RhythmVisualDriverTester : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        [Tooltip("Seconds from pressing Test until the first note appears at the spawn point.")]
        [SerializeField]
        private float timeUntilFirstSpawn = 1f;

        [SerializeField]
        private int noteCount = 8;

        [Tooltip("Seconds between consecutive notes' hit times.")]
        [SerializeField]
        private float noteInterval = 0.5f;

        [SerializeField]
        private float tailOut = 0.5f;

        [SerializeField]
        private RhythmTimingWindows windows = new RhythmTimingWindows
        {
            perfect = 0.05f,
            good = 0.1f,
            ok = 0.15f,
            holdReleaseTolerance = 0.1f
        };

        private RhythmVisualDriver _driver;
        private InputTokenPoller _poller;
        private RhythmSequenceRunner _runner;
        private RhythmSequenceDefinition _definition;
        private Action<InputToken> _onTokenPressed;
        private Action<InputToken> _onTokenReleased;

        public bool IsRunning => _runner != null && _runner.IsRunning;

        private void Awake()
        {
            _driver = GetComponent<RhythmVisualDriver>();
            _onTokenPressed = token => _runner?.Press(token);
            _onTokenReleased = token => _runner?.Release(token);
        }

        private void OnDisable()
        {
            _runner?.Abort();
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            _poller.Poll(_onTokenPressed, _onTokenReleased);
            _runner.Tick(Time.deltaTime);
        }

        public void RunTest()
        {
            if (inputMap == null || inputMap.Entries.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(RhythmVisualDriverTester)} on '{name}' needs an input map with at least one entry.");
            }

            _runner?.Abort();

            var notes = new RhythmNote[Mathf.Max(1, noteCount)];
            for (var i = 0; i < notes.Length; i++)
            {
                var entry = inputMap.Entries[UnityEngine.Random.Range(0, inputMap.Entries.Count)];
                notes[i] = new RhythmNote
                {
                    input = entry.token,
                    kind = RhythmNoteKind.Tap,
                    time = i * noteInterval
                };
            }

            // The driver spawns a note travelDuration before its hit time, so padding the
            // lead-in by that much makes the first note appear exactly timeUntilFirstSpawn in.
            var leadIn = timeUntilFirstSpawn + _driver.TravelDuration;
            _definition = RhythmSequenceDefinition.CreateRuntime(leadIn, tailOut, windows, notes);

            _poller = new InputTokenPoller(inputMap);
            _runner = new RhythmSequenceRunner(_definition);
            _runner.Completed += HandleCompleted;

            inputMap.EnableAll();
            _runner.Start();
            _driver.Show(_runner);
        }

        private void HandleCompleted(RhythmCompletionReport report)
        {
            _runner.Completed -= HandleCompleted;
            _driver.Hide();
            Destroy(_definition);
            _definition = null;

            Debug.Log($"[RhythmTest] {report.HitNotes}/{report.TotalNotes} hit " +
                      $"(P={report.PerfectCount} G={report.GoodCount} O={report.OkCount} M={report.MissCount}), " +
                      $"accuracy={report.AccuracyScore:F2}, stray={report.StrayPresses}, aborted={report.WasAborted}", this);
        }
    }
}
