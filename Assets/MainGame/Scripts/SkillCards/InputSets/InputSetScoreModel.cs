using System;
using GARA.InputSets;
using UnityEngine;

namespace GARA.SkillCards.InputSets
{
    // Authored knobs for turning one InputSetCompletionReport into a single
    // 0-1 SkillPerformance score. Kept as data (rather than hardcoded in
    // InputSetSkillCard) so different cards sharing the same set collection engine
    // can weigh first-try cleanliness, completion, and speed differently.
    [Serializable]
    public struct InputSetScoreModel
    {
        // 0 = pure AttemptEfficiency (how few retries it took), 1 = pure
        // FirstTryRate (fraction of sets cleared on the first attempt).
        [Range(0, 1)]
        public float firstTryWeight;

        // When true, the blended efficiency term is multiplied by
        // CompletionRate so an incomplete run can never outscore a complete
        // one purely on efficiency.
        public bool gateByCompletion;

        // Seconds a "perfect speed" run is measured against. 0 disables the
        // speed term entirely (speedWeight has no effect).
        [Min(0)]
        public float speedReferenceSeconds;

        // How much the speed term blends into the final score. 0 is a true
        // no-op regardless of speedReferenceSeconds.
        [Range(0, 1)]
        public float speedWeight;

        public static InputSetScoreModel Default => new InputSetScoreModel
        {
            firstTryWeight = 0f,
            gateByCompletion = true,
            speedReferenceSeconds = 0f,
            speedWeight = 0f
        };

        public float Evaluate(InputSetCompletionReport report)
        {
            if (report == null)
            {
                return 0f;
            }

            var efficiencyTerm = Mathf.Lerp(report.AttemptEfficiency, report.FirstTryRate, firstTryWeight);
            var score = efficiencyTerm;

            if (gateByCompletion)
            {
                score *= report.CompletionRate;
            }

            if (speedReferenceSeconds > 0f && speedWeight > 0f)
            {
                var speedFactor = Mathf.Clamp01(1f - report.TotalElapsed / speedReferenceSeconds);
                score = Mathf.Lerp(score, score * speedFactor, speedWeight);
            }

            return Mathf.Clamp01(score);
        }
    }
}
