using System;
using GARA.Input;
using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// Drives a <see cref="ShakeBalanceDefinition"/>: simulates the drifting value, applies pushes,
    /// banks quality and reports the result. Plain C# so it can be unit tested and driven by any
    /// host; pass a seed for a reproducible wobble.
    /// </summary>
    public sealed class ShakeBalanceRunner
    {
        private readonly ShakeBalanceDefinition _definition;
        private readonly System.Random _random;

        private float _value;
        private float _velocity;
        private float _noise;
        private float _noiseTarget;
        private float _noiseTimer;
        private float _elapsedTime;
        private float _bank;
        private float _perfectTime;
        private float _goodTime;
        private float _offTime;
        private int _pushes;
        private ShakeBalanceZone _zone;
        private bool _isRunning;
        private ShakeBalanceReport _report;

        public ShakeBalanceRunner(ShakeBalanceDefinition definition, int? seed = null)
        {
            _definition = definition;
            _random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>Raised on every accepted push: -1 for left, +1 for right.</summary>
        public event Action<int> Pushed;

        public event Action<ShakeBalanceZone> ZoneChanged;
        public event Action<ShakeBalanceReport> Completed;

        public ShakeBalanceDefinition Definition => _definition;
        public bool IsRunning => _isRunning;

        /// <summary>The balance value: 0 is centered, -1 / +1 are the edges.</summary>
        public float Value => _value;

        public ShakeBalanceZone Zone => _zone;
        public float ElapsedTime => _elapsedTime;
        public float Bank => _bank;
        public float Instability => _definition.InstabilityAt(_elapsedTime);

        /// <summary>What cashing out right now would keep.</summary>
        public float CurrentResult => _definition.ResultFor(_bank);

        public float TimeRemaining => _definition.Duration <= 0f
            ? float.PositiveInfinity
            : Mathf.Max(0f, _definition.Duration - _elapsedTime);

        /// <summary>Null until the run ends.</summary>
        public ShakeBalanceReport Report => _report;

        public void Start()
        {
            _value = 0f;
            _velocity = 0f;
            _noise = 0f;
            _noiseTarget = NextNoiseTarget();
            _noiseTimer = 0f;
            _elapsedTime = 0f;
            _bank = 0f;
            _perfectTime = 0f;
            _goodTime = 0f;
            _offTime = 0f;
            _pushes = 0;
            _zone = _definition.ZoneAt(0f);
            _report = null;
            _isRunning = true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!_isRunning || deltaSeconds <= 0f)
            {
                return;
            }

            _elapsedTime += deltaSeconds;

            UpdateNoise(deltaSeconds);

            _velocity *= Mathf.Exp(-_definition.PushDamping * deltaSeconds);
            var drift = _value * Instability + _noise * _definition.NoiseAt(_elapsedTime);
            _value += (_velocity + drift) * deltaSeconds;

            if (Mathf.Abs(_value) >= 1f)
            {
                _value = Mathf.Sign(_value);
                SetZone(ShakeBalanceZone.Off);
                Finish(ShakeBalanceEndReason.Fell);
                return;
            }

            SetZone(_definition.ZoneAt(_value));
            Accumulate(_zone, deltaSeconds);

            if (_definition.Duration > 0f && _elapsedTime >= _definition.Duration)
            {
                Finish(ShakeBalanceEndReason.TimeUp);
            }
        }

        public void Press(InputToken token)
        {
            if (!_isRunning)
            {
                return;
            }

            if (_definition.UseCashOutToken && token == _definition.CashOutToken)
            {
                Finish(ShakeBalanceEndReason.CashedOut);
                return;
            }

            int direction;
            if (token == _definition.LeftToken)
            {
                direction = -1;
            }
            else if (token == _definition.RightToken)
            {
                direction = 1;
            }
            else
            {
                return;
            }

            _velocity += direction * _definition.PushStrength;
            _pushes++;
            Pushed?.Invoke(direction);
        }

        /// <summary>Ends the run as a cash out — keeps the full result.</summary>
        public void Stop()
        {
            if (_isRunning)
            {
                Finish(ShakeBalanceEndReason.CashedOut);
            }
        }

        public void Abort()
        {
            if (_isRunning)
            {
                Finish(ShakeBalanceEndReason.Aborted);
            }
        }

        private void UpdateNoise(float deltaSeconds)
        {
            var interval = _definition.NoiseChangeInterval;
            _noiseTimer += deltaSeconds;
            while (_noiseTimer >= interval)
            {
                _noiseTimer -= interval;
                _noiseTarget = NextNoiseTarget();
            }

            _noise = Mathf.Lerp(_noise, _noiseTarget, 1f - Mathf.Exp(-deltaSeconds / interval));
        }

        private float NextNoiseTarget()
        {
            return (float)(_random.NextDouble() * 2.0 - 1.0);
        }

        private void Accumulate(ShakeBalanceZone zone, float deltaSeconds)
        {
            _bank += _definition.QualityOf(zone) * deltaSeconds;

            switch (zone)
            {
                case ShakeBalanceZone.Perfect:
                    _perfectTime += deltaSeconds;
                    break;
                case ShakeBalanceZone.Good:
                    _goodTime += deltaSeconds;
                    break;
                default:
                    _offTime += deltaSeconds;
                    break;
            }
        }

        private void SetZone(ShakeBalanceZone zone)
        {
            if (zone == _zone)
            {
                return;
            }

            _zone = zone;
            ZoneChanged?.Invoke(zone);
        }

        private void Finish(ShakeBalanceEndReason reason)
        {
            _isRunning = false;

            var unpenalized = CurrentResult;
            _report = new ShakeBalanceReport
            {
                Result = reason == ShakeBalanceEndReason.Fell ? unpenalized * _definition.KeepOnFall : unpenalized,
                UnpenalizedResult = unpenalized,
                Bank = _bank,
                Elapsed = _elapsedTime,
                AverageQuality = _elapsedTime > 0f ? _bank / _elapsedTime : 0f,
                PerfectTime = _perfectTime,
                GoodTime = _goodTime,
                OffTime = _offTime,
                Pushes = _pushes,
                FinalInstability = Instability,
                EndReason = reason
            };

            Completed?.Invoke(_report);
        }
    }
}
