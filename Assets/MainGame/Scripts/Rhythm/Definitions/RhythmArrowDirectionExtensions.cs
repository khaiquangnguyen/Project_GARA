namespace GARA.Rhythm
{
    public static class RhythmArrowDirectionExtensions
    {
        /// <summary>Z rotation in degrees (Unity 2D: counter-clockwise positive) of <paramref name="direction"/>, measured from Up.</summary>
        public static float ToDegrees(this RhythmArrowDirection direction)
        {
            switch (direction)
            {
                case RhythmArrowDirection.Left:
                    return 90f;
                case RhythmArrowDirection.Down:
                    return 180f;
                case RhythmArrowDirection.Right:
                    return -90f;
                default:
                    return 0f;
            }
        }
    }
}
