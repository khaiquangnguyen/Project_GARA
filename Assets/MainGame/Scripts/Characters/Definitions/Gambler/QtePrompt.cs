namespace GARA.Characters.Gambler
{
    // One ring of a QteRunner's run. Angles are degrees clockwise from 12.
    public sealed class QtePrompt
    {
        public QtePrompt(int index, float windowStart, float windowEnd)
        {
            Index = index;
            WindowStart = windowStart;
            WindowEnd = windowEnd;
        }

        public int Index { get; }

        public float WindowStart { get; }

        public float WindowEnd { get; }

        public float Angle { get; internal set; }

        public bool IsResolved { get; internal set; }

        public bool Hit { get; internal set; }

        public bool IsInWindow => Angle >= WindowStart && Angle <= WindowEnd;
    }
}
