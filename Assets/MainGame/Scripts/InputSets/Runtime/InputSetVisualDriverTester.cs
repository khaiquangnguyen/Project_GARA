using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// Play-mode test harness for an <see cref="InputSetVisualDriver"/>: generates a random
    /// set collection from <see cref="inputMap"/>'s tokens, runs and judges it against live
    /// input, and logs the completion report. Stands in for a character's
    /// <see cref="InputSetCollectionPlayer"/> (ticking and polling in the same order) so the driver
    /// prefab can be tuned without a combat scene.
    /// </summary>
    [RequireComponent(typeof(InputSetVisualDriver))]
    public class InputSetVisualDriverTester : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        [SerializeField]
        private int setCount = 4;

        [SerializeField]
        private int minInputsPerSet = 3;

        [SerializeField]
        private int maxInputsPerSet = 5;

        [Tooltip("Seconds per set attempt. 0 = no set time limit.")]
        [SerializeField]
        private float setTimeLimit = 4f;

        [Tooltip("Seconds for the whole set collection. 0 = no set collection time limit.")]
        [SerializeField]
        private float setCollectionTimeLimit;

        [SerializeField]
        private InputSetRetryPolicy retryPolicy = InputSetRetryPolicy.Default;

        private InputSetVisualDriver _driver;
        private InputTokenPoller _poller;
        private InputSetCollectionRunner _runner;
        private InputSetCollectionDefinition _definition;
        private Action<InputToken> _onTokenPressed;

        public bool IsRunning => _runner != null && _runner.IsRunning;

        private void Awake()
        {
            _driver = GetComponent<InputSetVisualDriver>();
            _onTokenPressed = token => _runner?.Press(token);
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

            _runner.Tick(Time.deltaTime);

            if (!_runner.IsRunning)
            {
                return;
            }

            _poller.Poll(_onTokenPressed, null);
        }

        public void RunTest()
        {
            if (inputMap == null || inputMap.Entries.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(InputSetVisualDriverTester)} on '{name}' needs an input map with at least one entry.");
            }

            _runner?.Abort();

            var minInputs = Mathf.Max(1, minInputsPerSet);
            var maxInputs = Mathf.Max(minInputs, maxInputsPerSet);
            var sets = new InputSetDefinition[Mathf.Max(1, setCount)];
            for (var i = 0; i < sets.Length; i++)
            {
                var inputs = new InputToken[UnityEngine.Random.Range(minInputs, maxInputs + 1)];
                for (var j = 0; j < inputs.Length; j++)
                {
                    inputs[j] = inputMap.Entries[UnityEngine.Random.Range(0, inputMap.Entries.Count)].token;
                }

                sets[i] = new InputSetDefinition
                {
                    inputs = inputs,
                    timeLimit = setTimeLimit
                };
            }

            _definition = InputSetCollectionDefinition.CreateRuntime(sets, setCollectionTimeLimit, retryPolicy);

            _poller = new InputTokenPoller(inputMap);
            _runner = new InputSetCollectionRunner(_definition);
            _runner.Completed += HandleCompleted;

            inputMap.EnableAll();

            // Shown before Start() so the driver catches the first SetStarted, matching
            // InputSetCollectionPlayer's AnySetCollectionStarted order.
            _driver.Show(_runner);
            _runner.Start();
        }

        private void HandleCompleted(InputSetCompletionReport report)
        {
            _runner.Completed -= HandleCompleted;
            _driver.Hide();
            Destroy(_definition);
            _definition = null;

            Debug.Log($"[InputSetTest] cleared {report.ClearedSets}/{report.TotalSets} sets " +
                      $"({report.CompletionRate:P0}), firstTry={report.FirstTryRate:P0}, " +
                      $"attempts={report.TotalAttempts}, aborted={report.WasAborted}, " +
                      $"setCollectionTimedOut={report.SetCollectionTimedOut}, elapsed={report.TotalElapsed:F2}s", this);
        }
    }
}
