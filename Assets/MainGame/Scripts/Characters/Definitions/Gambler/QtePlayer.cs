using System;
using System.Collections.Generic;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Polls input into a QteRunner. When not player-controlled, or on
    // AutoPlay, it plays itself: AI lands each prompt at the config's chance.
    public class QtePlayer : MonoBehaviour, IAutoPlayable
    {
        // AutoPlay RandomFail: chance each prompt lands.
        private const float RandomFailHitChance = 0.65f;

        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private readonly Dictionary<QtePrompt, float> _autoPressAngles = new Dictionary<QtePrompt, float>();

        private InputTokenPoller _poller;
        private QteRunner _runner;
        private Action<InputToken> _onTokenPressed;
        private Action<QteReport> _onCompleted;

        private bool _autoPlaying;

        public static event Action<QtePlayer> AnyStarted;

        public static event Action<QtePlayer> AnyEnded;

        public AutoPlayMode AutoPlay { get; set; }

        public bool IsPlaying => _runner != null && _runner.IsRunning;

        public QteRunner CurrentRunner => _runner;

        public InputTokenMap InputMap => inputMap;

        public void Play(Qte config, int count, bool playerControlled, Action<QteReport> onCompleted)
        {
            Abort();

            _poller = new InputTokenPoller(inputMap);
            _runner = new QteRunner(config, count);
            _onCompleted = onCompleted;
            _autoPlaying = !playerControlled || AutoPlay != AutoPlayMode.Off;
            _autoPressAngles.Clear();
            _runner.PromptStarted += HandlePromptStarted;
            _runner.Completed += HandleCompleted;

            inputMap?.EnableAll();

            AnyStarted?.Invoke(this);
            _runner.Start();
        }

        // Cuts the run short; onCompleted still fires, with an aborted report.
        public void Abort()
        {
            if (_runner == null)
            {
                return;
            }

            _runner.Abort();
            Unbind();
        }

        private void Awake()
        {
            _onTokenPressed = HandleTokenPressed;
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _runner.Tick(Time.deltaTime);
            var current = IsPlaying ? _runner.Current : null;
            if (current == null)
            {
                return;
            }

            if (!_autoPlaying)
            {
                _poller.Poll(_onTokenPressed, null);
                return;
            }

            if (_autoPressAngles.TryGetValue(current, out var pressAngle) && current.Angle >= pressAngle)
            {
                _autoPressAngles.Remove(current);
                _runner.Press(_runner.Config.token);
            }
        }

        private void OnDisable()
        {
            Abort();
        }

        // A hit presses inside the arc; a miss either jumps the gun or lets
        // the needle pass.
        private void HandlePromptStarted(QtePrompt prompt)
        {
            if (!_autoPlaying)
            {
                return;
            }

            if (UnityEngine.Random.value < AutoHitChance())
            {
                _autoPressAngles[prompt] = Mathf.Lerp(prompt.WindowStart, prompt.WindowEnd, UnityEngine.Random.Range(0.2f, 0.8f));
                return;
            }

            _autoPressAngles[prompt] = UnityEngine.Random.value < 0.5f ? Mathf.Max(0f, prompt.WindowStart - UnityEngine.Random.Range(10f, 40f)) : float.MaxValue;
        }

        private float AutoHitChance()
        {
            switch (AutoPlay)
            {
                case AutoPlayMode.Perfect:
                    return 1f;
                case AutoPlayMode.RandomFail:
                    return RandomFailHitChance;
                default:
                    return _runner.Config.aiSuccessChance;
            }
        }

        private void HandleTokenPressed(InputToken token)
        {
            _runner?.Press(token);
        }

        private void HandleCompleted(QteReport report)
        {
            var onCompleted = _onCompleted;
            Unbind();
            AnyEnded?.Invoke(this);
            onCompleted?.Invoke(report);
        }

        private void Unbind()
        {
            if (_runner == null)
            {
                return;
            }

            _runner.PromptStarted -= HandlePromptStarted;
            _runner.Completed -= HandleCompleted;
            _runner = null;
            _onCompleted = null;
        }
    }
}
