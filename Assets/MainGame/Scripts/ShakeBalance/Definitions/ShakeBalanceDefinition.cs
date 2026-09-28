using GARA.Input;
using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// A balance minigame with one goal: a value in [-1, 1] drifts away from the center on its
    /// own and two tokens push it left and right. Time near the center fills a meter; filling it
    /// before the duration runs out succeeds and ends the run. Running out of time or letting the
    /// value fall off an edge fails.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Shake Balance/Shake Balance", fileName = "ShakeBalance")]
    public class ShakeBalanceDefinition : ScriptableObject
    {
        [Header("Tokens")]
        [SerializeField]
        private InputToken leftToken;

        [SerializeField]
        private InputToken rightToken;

        [Header("Push")]
        [Tooltip("Velocity added by one tap. The distance a lone tap travels is Push Strength / Push Damping.")]
        [Min(0f)]
        [SerializeField]
        private float pushStrength = 1f;

        [Tooltip("How fast a tap's velocity dies out, per second. Higher = snappier, shorter pushes.")]
        [Min(0.01f)]
        [SerializeField]
        private float pushDamping = 8f;

        [Header("Drift")]
        [Tooltip("How strongly the value is pulled away from the center, per unit of distance, per second.")]
        [Min(0f)]
        [SerializeField]
        private float baseInstability = 1.2f;

        [Tooltip("Instability added per second of play — what keeps a long run from staying safe.")]
        [Min(0f)]
        [SerializeField]
        private float instabilityGrowth = 0.02f;

        [Tooltip("Strength of the random wobble, in value units per second.")]
        [Min(0f)]
        [SerializeField]
        private float baseNoise = 0.3f;

        [Tooltip("Noise strength added per second of play.")]
        [Min(0f)]
        [SerializeField]
        private float noiseGrowth = 0.02f;

        [Tooltip("Seconds between picks of a new random wobble direction. The wobble eases toward each pick.")]
        [Min(0.05f)]
        [SerializeField]
        private float noiseChangeInterval = 0.4f;

        [Header("Zones")]
        [Tooltip("Half-width of the perfect zone around the center. Quality 1 inside it.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float perfectZone = 0.15f;

        [Tooltip("Half-width of the good zone around the center. Must be at least the perfect zone.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float goodZone = 0.35f;

        [Header("Meter")]
        [Tooltip("Seconds in the perfect zone to fill the meter from empty.")]
        [Min(0.01f)]
        [SerializeField]
        private float perfectFillSeconds = 1.5f;

        [Tooltip("Seconds in the good (but not perfect) zone to fill the meter from empty.")]
        [Min(0.01f)]
        [SerializeField]
        private float goodFillSeconds = 3f;

        [Tooltip("Meter lost per second outside the good zone.")]
        [Min(0f)]
        [SerializeField]
        private float offDrainPerSecond = 0.25f;

        [Tooltip("Seconds to fill the meter before the run fails.")]
        [Min(0.1f)]
        [SerializeField]
        private float duration = 4f;

        public InputToken LeftToken => leftToken;
        public InputToken RightToken => rightToken;
        public float PushStrength => pushStrength;
        public float PushDamping => pushDamping;
        public float BaseInstability => baseInstability;
        public float InstabilityGrowth => instabilityGrowth;
        public float BaseNoise => baseNoise;
        public float NoiseGrowth => noiseGrowth;
        public float NoiseChangeInterval => noiseChangeInterval;
        public float PerfectZone => perfectZone;
        public float GoodZone => goodZone;
        public float PerfectFillSeconds => perfectFillSeconds;
        public float GoodFillSeconds => goodFillSeconds;
        public float OffDrainPerSecond => offDrainPerSecond;
        public float Duration => duration;

        /// <summary>Builds an in-memory definition (not saved as an asset) with default tuning — for tests and generated runs. Caller owns destroying it.</summary>
        public static ShakeBalanceDefinition CreateRuntime(InputToken leftToken, InputToken rightToken, float duration)
        {
            var definition = CreateInstance<ShakeBalanceDefinition>();
            definition.leftToken = leftToken;
            definition.rightToken = rightToken;
            definition.duration = Mathf.Max(0.1f, duration);
            return definition;
        }

        /// <summary>Overrides the difficulty knobs of a runtime definition, e.g. from a test harness.</summary>
        public void SetDifficulty(float baseInstability, float instabilityGrowth, float baseNoise, float noiseGrowth)
        {
            this.baseInstability = baseInstability;
            this.instabilityGrowth = instabilityGrowth;
            this.baseNoise = baseNoise;
            this.noiseGrowth = noiseGrowth;
        }

        /// <summary>Overrides the scoring knobs of a runtime definition, e.g. from a test harness.</summary>
        public void SetScoring(float perfectZone, float goodZone, float perfectFillSeconds, float goodFillSeconds, float offDrainPerSecond)
        {
            this.perfectZone = perfectZone;
            this.goodZone = Mathf.Max(perfectZone, goodZone);
            this.perfectFillSeconds = Mathf.Max(0.01f, perfectFillSeconds);
            this.goodFillSeconds = Mathf.Max(0.01f, goodFillSeconds);
            this.offDrainPerSecond = Mathf.Max(0f, offDrainPerSecond);
        }

        /// <summary>Instability after <paramref name="elapsed"/> seconds of play.</summary>
        public float InstabilityAt(float elapsed)
        {
            return baseInstability + instabilityGrowth * elapsed;
        }

        /// <summary>Noise strength after <paramref name="elapsed"/> seconds of play.</summary>
        public float NoiseAt(float elapsed)
        {
            return baseNoise + noiseGrowth * elapsed;
        }

        public ShakeBalanceZone ZoneAt(float value)
        {
            var distance = Mathf.Abs(value);
            if (distance <= perfectZone)
            {
                return ShakeBalanceZone.Perfect;
            }

            return distance <= goodZone ? ShakeBalanceZone.Good : ShakeBalanceZone.Off;
        }

        /// <summary>Meter gained per second in <paramref name="zone"/>; negative drains it.</summary>
        public float FillRateOf(ShakeBalanceZone zone)
        {
            switch (zone)
            {
                case ShakeBalanceZone.Perfect:
                    return 1f / perfectFillSeconds;
                case ShakeBalanceZone.Good:
                    return 1f / goodFillSeconds;
                default:
                    return -offDrainPerSecond;
            }
        }

        private void OnValidate()
        {
            if (goodZone < perfectZone)
            {
                goodZone = perfectZone;
            }

            if (leftToken == rightToken)
            {
                Debug.LogWarning($"[{name}] ShakeBalanceDefinition: left and right tokens are the same.", this);
            }
        }
    }
}
