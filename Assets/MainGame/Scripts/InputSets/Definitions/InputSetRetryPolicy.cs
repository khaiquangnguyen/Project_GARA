using System;

namespace GARA.InputSets
{
    /// <summary>
    /// What happens once a set has exhausted its allowed retry attempts.
    /// </summary>
    public enum ExhaustedSetBehaviour
    {
        Skip,
        EndSetCollection
    }

    [Serializable]
    public struct InputSetRetryPolicy
    {
        public bool resetTimerOnRetry;
        public int maxAttemptsPerSet;
        public ExhaustedSetBehaviour onExhausted;

        public static InputSetRetryPolicy Default => new InputSetRetryPolicy
        {
            resetTimerOnRetry = true,
            maxAttemptsPerSet = 0,
            onExhausted = ExhaustedSetBehaviour.Skip
        };
    }
}
