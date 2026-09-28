using System;
using GARA.Input;

namespace GARA.Characters.Gambler
{
    // Plain C# rules of one cheat shake run. Each switch of side counts one;
    // the same side twice resets the count; one past Required fails the run.
    public sealed class CheatShakeRunner
    {
        private readonly CheatShake _config;

        // -1 left, +1 right, 0 before the first press or after a reset.
        private int _lastSide;

        public CheatShakeRunner(CheatShake config, int required)
        {
            _config = config;
            Required = required;
        }

        // Side of each counted press: -1 left, +1 right.
        public event Action<int> Pushed;

        public event Action CountReset;

        public event Action<CheatShakeReport> Completed;

        public CheatShake Config => _config;

        public int Required { get; }

        public int Count { get; private set; }

        public float Elapsed { get; private set; }

        public float TimeLimit => Required * _config.secondsPerInput;

        public float TimeLeft01 => HasStarted ? Math.Max(0f, 1f - Elapsed / TimeLimit) : 1f;

        // The timer starts on the first side press.
        public bool HasStarted { get; private set; }

        public bool IsRunning { get; private set; }

        // Side that counts next; 0 = either.
        public int NextSide => -_lastSide;

        public void Start()
        {
            Count = 0;
            Elapsed = 0f;
            _lastSide = 0;
            HasStarted = false;
            IsRunning = true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsRunning || !HasStarted)
            {
                return;
            }

            Elapsed += deltaSeconds;
            if (Elapsed >= TimeLimit)
            {
                Finish(CheatShakeEndReason.TimeUp);
            }
        }

        public void Press(InputToken token)
        {
            if (!IsRunning)
            {
                return;
            }

            if (token == _config.doneToken)
            {
                if (HasStarted)
                {
                    Finish(CheatShakeEndReason.Done);
                }

                return;
            }

            var side = token == _config.leftToken ? -1 : token == _config.rightToken ? 1 : 0;
            if (side == 0)
            {
                return;
            }

            HasStarted = true;
            if (side == _lastSide)
            {
                Count = 0;
                _lastSide = 0;
                CountReset?.Invoke();
                return;
            }

            _lastSide = side;
            Count++;
            if (Count > Required)
            {
                Finish(CheatShakeEndReason.TooMany);
                return;
            }

            Pushed?.Invoke(side);
        }

        public void Abort()
        {
            if (IsRunning)
            {
                Finish(CheatShakeEndReason.Aborted);
            }
        }

        private void Finish(CheatShakeEndReason reason)
        {
            IsRunning = false;
            Completed?.Invoke(new CheatShakeReport(Required, Count, Elapsed, reason));
        }
    }
}
