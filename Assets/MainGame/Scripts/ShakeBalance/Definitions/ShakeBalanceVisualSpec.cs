using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// Every presentation-only value of the shake balance visual system, kept out of the prefabs
    /// so tweaks made during a Play-mode test persist. Nothing here affects judging — that stays
    /// in <see cref="ShakeBalanceDefinition"/>. Bars (result, timer) are MMHealthBars and keep
    /// their own colors.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Shake Balance/Shake Balance Visual Spec", fileName = "ShakeBalanceVisualSpec")]
    public class ShakeBalanceVisualSpec : ScriptableObject
    {
        [Header("Track")]
        [Tooltip("World-unit distance from the track's center to either edge — where the needle sits at a value of -1 / +1.")]
        [Min(0f)]
        [SerializeField]
        private float trackHalfWidth = 3f;

        [Tooltip("Z rotation of the optional tilt target at a value of +1 (clockwise). Mirrored for -1.")]
        [SerializeField]
        private float maxTiltDegrees = 30f;

        [Header("Needle")]
        [SerializeField]
        private Color perfectColor = Color.green;

        [SerializeField]
        private Color goodColor = Color.yellow;

        [SerializeField]
        private Color offColor = Color.red;

        public float TrackHalfWidth => trackHalfWidth;

        /// <summary>Tilt target Z rotation for a balance <paramref name="value"/> (-1 to 1).</summary>
        public Quaternion TiltFor(float value)
        {
            return Quaternion.Euler(0f, 0f, -value * maxTiltDegrees);
        }

        public Color ColorFor(ShakeBalanceZone zone)
        {
            switch (zone)
            {
                case ShakeBalanceZone.Perfect:
                    return perfectColor;
                case ShakeBalanceZone.Good:
                    return goodColor;
                default:
                    return offColor;
            }
        }
    }
}
