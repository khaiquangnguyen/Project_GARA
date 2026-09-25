using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// Play-mode test harness for a <see cref="ShakeBalanceVisualDriver"/>: builds a runtime
    /// balance definition from the tokens and tuning below, runs it against live input, and logs
    /// the report. Stands in for a character's <see cref="ShakeBalancePlayer"/> (ticking and
    /// polling in the same order) so the driver prefab can be tuned without a combat scene.
    /// </summary>
    [RequireComponent(typeof(ShakeBalanceVisualDriver))]
    public class ShakeBalanceVisualDriverTester : MonoBehaviour
    {
        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        [SerializeField]
        private InputToken leftToken = new InputToken(2);

        [SerializeField]
        private InputToken rightToken = new InputToken(4);

        [SerializeField]
        private bool useCashOutToken = true;

        [SerializeField]
        private InputToken cashOutToken = new InputToken(1);

        [Tooltip("Seconds until the run ends on its own. 0 = no limit.")]
        [SerializeField]
        private float duration;

        [SerializeField]
        private float baseInstability = 0.8f;

        [SerializeField]
        private float instabilityGrowth = 0.02f;

        [SerializeField]
        private float baseNoise = 0.3f;

        [SerializeField]
        private float noiseGrowth = 0.02f;

        [Range(0f, 1f)]
        [SerializeField]
        private float perfectZone = 0.15f;

        [Range(0f, 1f)]
        [SerializeField]
        private float goodZone = 0.35f;

        [SerializeField]
        private float bankTimeConstant = 8f;

        [Range(0f, 1f)]
        [SerializeField]
        private float keepOnFall = 0.5f;

        private ShakeBalanceVisualDriver _driver;
        private InputTokenPoller _poller;
        private ShakeBalanceRunner _runner;
        private ShakeBalanceDefinition _definition;
        private Action<InputToken> _onTokenPressed;

        public bool IsRunning => _runner != null && _runner.IsRunning;

        private void Awake()
        {
            _driver = GetComponent<ShakeBalanceVisualDriver>();
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
                throw new InvalidOperationException($"{nameof(ShakeBalanceVisualDriverTester)} on '{name}' needs an input map with at least one entry.");
            }

            _runner?.Abort();

            _definition = ShakeBalanceDefinition.CreateRuntime(leftToken, rightToken, useCashOutToken ? cashOutToken : (InputToken?)null, duration);
            _definition.SetDifficulty(baseInstability, instabilityGrowth, baseNoise, noiseGrowth);
            _definition.SetScoring(perfectZone, goodZone, bankTimeConstant, keepOnFall);

            _poller = new InputTokenPoller(inputMap);
            _runner = new ShakeBalanceRunner(_definition);

            inputMap.EnableAll();

            // Shown before subscribing and before Start(), matching ShakeBalancePlayer's order: the
            // driver handles Completed (end feedbacks) before HandleCompleted hides it.
            _driver.Show(_runner);
            _runner.Completed += HandleCompleted;
            _runner.Start();
        }

        private void HandleCompleted(ShakeBalanceReport report)
        {
            _runner.Completed -= HandleCompleted;
            _driver.Hide();
            Destroy(_definition);
            _definition = null;

            Debug.Log($"[ShakeBalanceTest] {ShakeBalanceDebugLogger.Describe(report)}", this);
        }
    }
}
