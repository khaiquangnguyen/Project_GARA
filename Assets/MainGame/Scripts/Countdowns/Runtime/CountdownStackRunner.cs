using System;
using System.Collections.Generic;
using GARA.Input;

namespace GARA.Countdowns
{
    /// <summary>
    /// Drives a class-agnostic stack of concurrent countdowns resolved by a single shared button.
    /// Plain C# — host it from a MonoBehaviour driver (see <see cref="CountdownStackPlayer"/>).
    /// </summary>
    public sealed class CountdownStackRunner
    {
        private sealed class InternalCountdown
        {
            public int id;
            public CountdownSpec spec;
            public float spawnTime;
            public float remaining;
            public bool wasAuthored;
            public bool perfectWindowEnteredRaised;
        }

        private readonly List<InternalCountdown> _active = new List<InternalCountdown>();
        private readonly List<CountdownResult> _results = new List<CountdownResult>();

        private int _nextId;
        private int _nextAuthoredSpawnIndex;
        private bool _hasBeenEmptySinceSpawns;
        private float _emptySince;

        public CountdownStackRunner(CountdownStackDefinition definition)
        {
            Definition = definition;
        }

        public bool IsRunning { get; private set; }
        public float ElapsedTime { get; private set; }
        public CountdownStackDefinition Definition { get; }

        public IReadOnlyList<ActiveCountdown> Active
        {
            get
            {
                var ordered = new List<InternalCountdown>(_active);
                ordered.Sort(CompareByPriority);

                var snapshot = new List<ActiveCountdown>(ordered.Count);
                foreach (var countdown in ordered)
                {
                    snapshot.Add(ToActiveCountdown(countdown));
                }

                return snapshot;
            }
        }

        public int SuccessCount { get; private set; }
        public int PerfectCount { get; private set; }
        public int NormalCount { get; private set; }
        public int MissCount { get; private set; }
        public int WhiffPresses { get; private set; }

        public event Action<ActiveCountdown> CountdownSpawned;
        public event Action<int> PerfectWindowEntered;
        public event Action<CountdownResult> CountdownResolved;
        public event Action<CountdownResult> CountdownExpired;
        public event Action Whiffed;
        public event Action<CountdownStackReport> Completed;

        public int GetSuccessCount()
        {
            return SuccessCount;
        }

        public CountdownStackReport Snapshot()
        {
            return BuildReport(false);
        }

        public void Start()
        {
            _active.Clear();
            _results.Clear();
            _nextId = 0;
            _nextAuthoredSpawnIndex = 0;
            _hasBeenEmptySinceSpawns = false;
            _emptySince = 0f;
            ElapsedTime = 0f;
            SuccessCount = 0;
            PerfectCount = 0;
            NormalCount = 0;
            MissCount = 0;
            WhiffPresses = 0;
            IsRunning = true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsRunning)
            {
                return;
            }

            ElapsedTime += deltaSeconds;

            var authoredSpawns = Definition.authoredSpawns;
            while (_nextAuthoredSpawnIndex < authoredSpawns.Length && authoredSpawns[_nextAuthoredSpawnIndex].spawnTime <= ElapsedTime)
            {
                SpawnInternal(authoredSpawns[_nextAuthoredSpawnIndex].spec, true);
                _nextAuthoredSpawnIndex++;
            }

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var countdown = _active[i];
                countdown.remaining -= deltaSeconds;

                if (countdown.remaining <= 0f)
                {
                    _active.RemoveAt(i);
                    var result = new CountdownResult(countdown.id, CountdownOutcome.Missed, countdown.spawnTime, countdown.spec.duration, float.NaN, countdown.wasAuthored);
                    _results.Add(result);
                    MissCount++;
                    CountdownExpired?.Invoke(result);
                    continue;
                }

                var inPerfectWindow = IsInPerfectWindow(countdown);
                if (inPerfectWindow && !countdown.perfectWindowEnteredRaised)
                {
                    countdown.perfectWindowEnteredRaised = true;
                    PerfectWindowEntered?.Invoke(countdown.id);
                }
            }

            if (!Definition.runUntilStopped && _nextAuthoredSpawnIndex >= authoredSpawns.Length && _active.Count == 0)
            {
                if (!_hasBeenEmptySinceSpawns)
                {
                    _hasBeenEmptySinceSpawns = true;
                    _emptySince = ElapsedTime;
                }
                else if (ElapsedTime - _emptySince >= Definition.tailOut)
                {
                    var report = BuildReport(false);
                    IsRunning = false;
                    Completed?.Invoke(report);
                }
            }
            else
            {
                _hasBeenEmptySinceSpawns = false;
            }
        }

        public void Press(InputToken token)
        {
            if (!IsRunning || token != Definition.button)
            {
                return;
            }

            InternalCountdown target = null;

            foreach (var countdown in _active)
            {
                if (!IsInPerfectWindow(countdown))
                {
                    continue;
                }

                if (target == null || IsHigherPriority(countdown, target))
                {
                    target = countdown;
                }
            }

            if (target != null)
            {
                ResolveCountdown(target, CountdownOutcome.Perfect);
                return;
            }

            foreach (var countdown in _active)
            {
                if (target == null || IsHigherPriority(countdown, target))
                {
                    target = countdown;
                }
            }

            if (target != null)
            {
                ResolveCountdown(target, CountdownOutcome.Normal);
                return;
            }

            WhiffPresses++;
            Whiffed?.Invoke();
        }

        public int Spawn()
        {
            return IsRunning ? SpawnInternal(Definition.defaultSpec, false) : -1;
        }

        public int Spawn(CountdownSpec spec)
        {
            return IsRunning ? SpawnInternal(spec, false) : -1;
        }

        public bool Cancel(int id)
        {
            for (var i = 0; i < _active.Count; i++)
            {
                var countdown = _active[i];
                if (countdown.id != id)
                {
                    continue;
                }

                _active.RemoveAt(i);
                var result = new CountdownResult(countdown.id, CountdownOutcome.Cancelled, countdown.spawnTime, countdown.spec.duration, countdown.remaining, countdown.wasAuthored);
                _results.Add(result);
                return true;
            }

            return false;
        }

        public void Stop()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var countdown = _active[i];
                _active.RemoveAt(i);
                _results.Add(new CountdownResult(countdown.id, CountdownOutcome.Cancelled, countdown.spawnTime, countdown.spec.duration, countdown.remaining, countdown.wasAuthored));
            }

            var report = BuildReport(true);
            IsRunning = false;
            Completed?.Invoke(report);
        }

        private int SpawnInternal(CountdownSpec spec, bool wasAuthored)
        {
            var countdown = new InternalCountdown
            {
                id = _nextId++,
                spec = spec,
                spawnTime = ElapsedTime,
                remaining = spec.duration,
                wasAuthored = wasAuthored
            };

            _active.Add(countdown);
            CountdownSpawned?.Invoke(ToActiveCountdown(countdown));
            return countdown.id;
        }

        private void ResolveCountdown(InternalCountdown countdown, CountdownOutcome outcome)
        {
            _active.Remove(countdown);
            var result = new CountdownResult(countdown.id, outcome, countdown.spawnTime, countdown.spec.duration, countdown.remaining, countdown.wasAuthored);
            _results.Add(result);

            if (outcome == CountdownOutcome.Perfect)
            {
                PerfectCount++;
            }
            else
            {
                NormalCount++;
            }

            SuccessCount++;
            CountdownResolved?.Invoke(result);
        }

        private static bool IsInPerfectWindow(InternalCountdown countdown)
        {
            var low = countdown.spec.perfectOffsetFromEnd;
            var high = countdown.spec.perfectOffsetFromEnd + countdown.spec.perfectWindow;
            return countdown.remaining >= low && countdown.remaining <= high;
        }

        private bool IsHigherPriority(InternalCountdown candidate, InternalCountdown current)
        {
            return CompareByPriority(candidate, current) < 0;
        }

        private int CompareByPriority(InternalCountdown a, InternalCountdown b)
        {
            switch (Definition.priority)
            {
                case CountdownPriority.OldestSpawned:
                    return a.id.CompareTo(b.id);
                case CountdownPriority.NewestSpawned:
                    return b.id.CompareTo(a.id);
                case CountdownPriority.SoonestToExpire:
                default:
                    return a.remaining.CompareTo(b.remaining);
            }
        }

        private static ActiveCountdown ToActiveCountdown(InternalCountdown countdown)
        {
            return new ActiveCountdown(countdown.id, countdown.remaining, countdown.spec.duration, IsInPerfectWindow(countdown), countdown.spec.perfectWindow, countdown.spec.perfectOffsetFromEnd);
        }

        private CountdownStackReport BuildReport(bool wasStopped)
        {
            var perfectCount = 0;
            var normalCount = 0;
            var missCount = 0;
            var cancelledCount = 0;

            foreach (var result in _results)
            {
                switch (result.Outcome)
                {
                    case CountdownOutcome.Perfect:
                        perfectCount++;
                        break;
                    case CountdownOutcome.Normal:
                        normalCount++;
                        break;
                    case CountdownOutcome.Missed:
                        missCount++;
                        break;
                    case CountdownOutcome.Cancelled:
                        cancelledCount++;
                        break;
                }
            }

            var total = _results.Count;
            var denom = total - cancelledCount;
            var successCount = perfectCount + normalCount;

            return new CountdownStackReport
            {
                TotalCountdowns = total,
                SuccessCount = successCount,
                PerfectCount = perfectCount,
                NormalCount = normalCount,
                MissCount = missCount,
                CancelledCount = cancelledCount,
                WhiffPresses = WhiffPresses,
                SuccessRate = denom > 0 ? (float)successCount / denom : 0f,
                PerfectRate = denom > 0 ? (float)perfectCount / denom : 0f,
                Elapsed = ElapsedTime,
                WasStopped = wasStopped,
                Results = new List<CountdownResult>(_results)
            };
        }
    }
}
