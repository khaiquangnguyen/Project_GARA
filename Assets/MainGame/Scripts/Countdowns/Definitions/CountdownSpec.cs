using System;

namespace GARA.Countdowns
{
    /// <summary>
    /// Describes a single countdown's lifetime and the "perfect" band near its expiry.
    /// The perfect band is the remaining-time range [perfectOffsetFromEnd, perfectOffsetFromEnd + perfectWindow];
    /// the default offset of 0 makes it the final perfectWindow seconds before expiry.
    /// </summary>
    [Serializable]
    public struct CountdownSpec
    {
        public float duration;
        public float perfectWindow;
        public float perfectOffsetFromEnd;
    }
}
