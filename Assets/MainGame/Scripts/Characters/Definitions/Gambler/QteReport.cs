using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    public sealed class QteReport
    {
        // One entry per resolved prompt, in order: true = well timed.
        public IReadOnlyList<bool> Results { get; }

        public int Count { get; }

        public int Hits { get; }

        public bool WasAborted { get; }

        public bool AllHit => !WasAborted && Count > 0 && Hits == Count;

        public QteReport(IReadOnlyList<bool> results, int count, bool wasAborted)
        {
            Results = results;
            Count = count;
            WasAborted = wasAborted;
            foreach (var hit in results)
            {
                if (hit)
                {
                    Hits++;
                }
            }
        }
    }
}
