using System;
using GARA.Input;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Polls input into a CheatShakeRunner. When not player-controlled it
    // plays itself, landing the exact count with the config's AI chance.
    public class CheatShakePlayer : MonoBehaviour
    {
        private const float AutoPressInterval = 0.1f;

        [SerializeField]
        [Expandable]
        private InputTokenMap inputMap;

        private InputTokenPoller _poller;
        private CheatShakeRunner _runner;
        private Action<InputToken> _onTokenPressed;
        private Action<CheatShakeReport> _onCompleted;

        private bool _autoPlaying;
        private int _autoTarget;
        private int _autoSide;
        private float _autoTimer;

        public static event Action<CheatShakePlayer> AnyStarted;

        public static event Action<CheatShakePlayer> AnyEnded;

        public bool IsPlaying => _runner != null && _runner.IsRunning;

        public CheatShakeRunner CurrentRunner => _runner;

        public InputTokenMap InputMap => inputMap;

        public void Play(CheatShake config, bool playerControlled, Action<CheatShakeReport> onCompleted)
        {
            Abort();

            _poller = new InputTokenPoller(inputMap);
            _runner = new CheatShakeRunner(config, config.RollRequired());
            _onCompleted = onCompleted;
            _runner.Completed += HandleCompleted;

            inputMap?.EnableAll();
            PrepareAutoPlay(playerControlled);

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
            _runner.Completed -= HandleCompleted;
            _runner = null;
            _onCompleted = null;
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
            if (!IsPlaying)
            {
                return;
            }

            if (!_autoPlaying)
            {
                _poller.Poll(_onTokenPressed, null);
                return;
            }

            _autoTimer -= Time.deltaTime;
            if (_autoTimer <= 0f)
            {
                _autoTimer = AutoInterval();
                AutoPress();
            }
        }

        private void OnDisable()
        {
            Abort();
        }

        private void PrepareAutoPlay(bool playerControlled)
        {
            _autoPlaying = !playerControlled;
            if (!_autoPlaying)
            {
                return;
            }

            var required = _runner.Required;
            var lands = UnityEngine.Random.value < _runner.Config.aiSuccessChance;
            _autoTarget = lands ? required : Mathf.Max(1, required - UnityEngine.Random.Range(1, 3));
            _autoSide = UnityEngine.Random.value < 0.5f ? -1 : 1;
            _autoTimer = AutoInterval();
        }

        // Fast enough to finish well inside the time limit.
        private float AutoInterval()
        {
            return Mathf.Min(AutoPressInterval, _runner.Config.secondsPerInput * 0.6f);
        }

        private void AutoPress()
        {
            var config = _runner.Config;
            if (_runner.Count < _autoTarget)
            {
                _runner.Press(_autoSide < 0 ? config.leftToken : config.rightToken);
                _autoSide = -_autoSide;
                return;
            }

            _runner.Press(config.doneToken);
        }

        private void HandleTokenPressed(InputToken token)
        {
            _runner?.Press(token);
        }

        private void HandleCompleted(CheatShakeReport report)
        {
            AnyEnded?.Invoke(this);
            _onCompleted?.Invoke(report);
        }
    }
}
