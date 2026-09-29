using System;
using System.Collections.Generic;
using GARA.Input;

namespace GARA.Characters.Gambler
{
    // Plain C# rules of a run of Count prompts. One at a time, or with
    // overlapInterval a new one every interval, stacking. A press resolves
    // the oldest open prompt: a hit if its needle is in the arc. A needle
    // passing its arc misses.
    public sealed class QteRunner
    {
        private readonly Qte _config;
        private readonly List<QtePrompt> _open = new List<QtePrompt>();
        private readonly List<bool> _results = new List<bool>();

        private float _untilNext;
        private float _untilFinish;

        public QteRunner(Qte config, int count)
        {
            _config = config;
            Count = Math.Max(1, count);
        }

        public event Action<QtePrompt> PromptStarted;

        public event Action<QtePrompt> PromptResolved;

        public event Action<QteReport> Completed;

        public Qte Config => _config;

        public int Count { get; }

        public int Started { get; private set; }

        public int Hits { get; private set; }

        // Unresolved prompts, oldest first.
        public IReadOnlyList<QtePrompt> Open => _open;

        // The prompt a press resolves; null between prompts.
        public QtePrompt Current => _open.Count > 0 ? _open[0] : null;

        public bool IsRunning { get; private set; }

        private bool Overlaps => _config.overlapInterval > 0f;

        public void Start()
        {
            _open.Clear();
            _results.Clear();
            Started = 0;
            Hits = 0;
            IsRunning = true;
            StartPrompt();
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsRunning)
            {
                return;
            }

            var sweep = 360f * deltaSeconds / _config.sweepSeconds;
            foreach (var prompt in _open)
            {
                prompt.Angle += sweep;
            }

            for (var i = 0; i < _open.Count;)
            {
                if (_open[i].Angle > _open[i].WindowEnd)
                {
                    Resolve(_open[i], false);
                }
                else
                {
                    i++;
                }
            }

            if (Started < Count)
            {
                if (Overlaps || _open.Count == 0)
                {
                    _untilNext -= deltaSeconds;
                    if (_untilNext <= 0f)
                    {
                        StartPrompt();
                    }
                }

                return;
            }

            if (_open.Count == 0)
            {
                _untilFinish -= deltaSeconds;
                if (_untilFinish <= 0f)
                {
                    Finish(false);
                }
            }
        }

        public void Press(InputToken token)
        {
            if (IsRunning && token == _config.token && Current != null)
            {
                Resolve(Current, Current.IsInWindow);
            }
        }

        public void Abort()
        {
            if (IsRunning)
            {
                Finish(true);
            }
        }

        private void StartPrompt()
        {
            var windowStart = _config.RollWindowStart();
            var prompt = new QtePrompt(Started, windowStart, windowStart + _config.windowDegrees);
            Started++;
            _open.Add(prompt);
            _untilNext = _config.overlapInterval;
            PromptStarted?.Invoke(prompt);
        }

        private void Resolve(QtePrompt prompt, bool hit)
        {
            _open.Remove(prompt);
            prompt.IsResolved = true;
            prompt.Hit = hit;
            _results.Add(hit);
            if (hit)
            {
                Hits++;
            }

            if (!Overlaps)
            {
                _untilNext = _config.secondsBetween;
            }

            _untilFinish = _config.secondsBetween;
            PromptResolved?.Invoke(prompt);
        }

        private void Finish(bool aborted)
        {
            IsRunning = false;
            _open.Clear();
            Completed?.Invoke(new QteReport(_results.ToArray(), Count, aborted));
        }
    }
}
