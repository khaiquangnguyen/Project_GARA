using System;

namespace GARA.Characters
{
    // Which formula a StatScaling uses to turn one input number (a combat
    // stat's current value, or a skill's performance score) into an output
    // magnitude/multiplier/duration/whatever the caller treats it as.
    public enum StatScalingKind
    {
        // Ignores the input entirely, always returns flatValue.
        Flat,

        // output = linearBase + linearSlope * (statValue - linearPivot).
        Linear,

        // output = statValue >= booleanThreshold ? onValue : offValue.
        Boolean,

        // output = the outputValue of the highest breakpoint whose
        // minValue <= statValue.
        Categorical
    }

    // One entry in a Categorical StatScaling's step function.
    [Serializable]
    public struct StatScalingBreakpoint
    {
        // The input value at (and above) which this breakpoint's outputValue
        // takes effect.
        public float minValue;

        // The value Evaluate returns once statValue reaches minValue, until
        // a higher-minValue breakpoint takes over.
        public float outputValue;
    }

    // A single interchangeable scaling model for turning a stat/score value
    // into a magnitude. Lets an effect author choose Flat/Linear/Boolean/
    // Categorical per-field instead of every effect hardcoding one formula.
    [Serializable]
    public struct StatScaling
    {
        public StatScalingKind kind;

        // Flat: the constant output value, regardless of statValue.
        public float flatValue;

        // Linear: output at linearPivot, before the slope is applied.
        public float linearBase;

        // Linear: change in output per unit statValue moves away from linearPivot.
        public float linearSlope;

        // Linear: the statValue at which output equals linearBase exactly.
        public float linearPivot;

        // Boolean: the statValue at and above which onValue is returned instead of offValue.
        public float booleanThreshold;

        // Boolean: the output once statValue >= booleanThreshold.
        public float onValue;

        // Boolean: the output while statValue < booleanThreshold.
        public float offValue;

        // Categorical: step-function breakpoints. Assumed sorted ascending
        // by minValue; Evaluate sorts defensively if they aren't.
        public StatScalingBreakpoint[] breakpoints;

        public float Evaluate(float statValue)
        {
            switch (kind)
            {
                case StatScalingKind.Flat:
                    return flatValue;

                case StatScalingKind.Linear:
                    return linearBase + linearSlope * (statValue - linearPivot);

                case StatScalingKind.Boolean:
                    return statValue >= booleanThreshold ? onValue : offValue;

                case StatScalingKind.Categorical:
                    return EvaluateCategorical(statValue);

                default:
                    return 0f;
            }
        }

        private float EvaluateCategorical(float statValue)
        {
            if (breakpoints == null || breakpoints.Length == 0)
            {
                return 0f;
            }

            var sorted = (StatScalingBreakpoint[])breakpoints.Clone();
            Array.Sort(sorted, (a, b) => a.minValue.CompareTo(b.minValue));

            // Defensive default: if statValue is below every breakpoint,
            // floor to the lowest breakpoint's outputValue rather than 0.
            var result = sorted[0].outputValue;

            foreach (var breakpoint in sorted)
            {
                if (breakpoint.minValue > statValue)
                {
                    break;
                }

                result = breakpoint.outputValue;
            }

            return result;
        }
    }
}
