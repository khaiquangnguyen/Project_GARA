using GARA.Input;
using UnityEngine;

namespace GARA.ShakeBalance
{
    /// <summary>
    /// A balance minigame: a value in [-1, 1] drifts away from the center on its own, harder the
    /// longer the run lasts, and two tokens push it left and right. Time spent near the center
    /// banks quality, and the result is that bank on a saturating curve — so the player can keep
    /// going as long as they like, but the result only ever approaches 1. The run ends when the
    /// player cashes out (keeps the full result), the value falls off an edge (keeps
    /// <see cref="KeepOnFall"/> of it), or the optional duration runs out.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Shake Balance/Shake Balance", fileName = "ShakeBalance")]
    public class ShakeBalanceDefinition : ScriptableObject
    {
        [Header("Tokens")]
        [SerializeField]
        private InputToken leftToken;

        [SerializeField]
        private InputToken rightToken;

        [Tooltip("When on, pressing the cash out token ends the run and keeps the full result. When off, the run only ends by falling, by the duration, or externally.")]
        [SerializeField]
        private bool useCashOutToken = true;

        [SerializeField]
        private InputToken cashOutToken;

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
        private float baseInstability = 0.8f;

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

        [Tooltip("Quality banked per second while in the good (but not perfect) zone. Outside it banks nothing.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float goodZoneQuality = 0.5f;

        [Header("Result")]
        [Tooltip("Seconds of perfect play for the result to reach ~63%. ~3x this reaches ~95%. The result never reaches 100%.")]
        [Min(0.01f)]
        [SerializeField]
        private float bankTimeConstant = 8f;

        [Tooltip("Fraction of the result kept when the value falls off an edge.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float keepOnFall = 0.5f;

        [Tooltip("Seconds until the run ends on its own, keeping the full result. 0 = no limit.")]
        [Min(0f)]
        [SerializeField]
        private float duration;

        public InputToken LeftToken => leftToken;
        public InputToken RightToken => rightToken;
        public bool UseCashOutToken => useCashOutToken;
        public InputToken CashOutToken => cashOutToken;
        public float PushStrength => pushStrength;
        public float PushDamping => pushDamping;
        public float BaseInstability => baseInstability;
        public float InstabilityGrowth => instabilityGrowth;
        public float BaseNoise => baseNoise;
        public float NoiseGrowth => noiseGrowth;
        public float NoiseChangeInterval => noiseChangeInterval;
        public float PerfectZone => perfectZone;
        public float GoodZone => goodZone;
        public float GoodZoneQuality => goodZoneQuality;
        public float BankTimeConstant => bankTimeConstant;
        public float KeepOnFall => keepOnFall;
        public float Duration => duration;

        /// <summary>Builds an in-memory definition (not saved as an asset) with default tuning — for tests and generated runs. Caller owns destroying it.</summary>
        public static ShakeBalanceDefinition CreateRuntime(InputToken leftToken, InputToken rightToken, InputToken? cashOutToken, float duration)
        {
            var definition = CreateInstance<ShakeBalanceDefinition>();
            definition.leftToken = leftToken;
            definition.rightToken = rightToken;
            definition.useCashOutToken = cashOutToken.HasValue;
            definition.cashOutToken = cashOutToken ?? default;
            definition.duration = duration;
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
        public void SetScoring(float perfectZone, float goodZone, float bankTimeConstant, float keepOnFall)
        {
            this.perfectZone = perfectZone;
            this.goodZone = Mathf.Max(perfectZone, goodZone);
            this.bankTimeConstant = Mathf.Max(0.01f, bankTimeConstant);
            this.keepOnFall = keepOnFall;
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

        /// <summary>Result (0 to just under 1) for a quality bank of <paramref name="bank"/> seconds.</summary>
        public float ResultFor(float bank)
        {
            return 1f - Mathf.Exp(-Mathf.Max(0f, bank) / bankTimeConstant);
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

        public float QualityOf(ShakeBalanceZone zone)
        {
            switch (zone)
            {
                case ShakeBalanceZone.Perfect:
                    return 1f;
                case ShakeBalanceZone.Good:
                    return goodZoneQuality;
                default:
                    return 0f;
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

            if (useCashOutToken && (cashOutToken == leftToken || cashOutToken == rightToken))
            {
                Debug.LogWarning($"[{name}] ShakeBalanceDefinition: the cash out token is also a push token.", this);
            }
        }
    }
}
