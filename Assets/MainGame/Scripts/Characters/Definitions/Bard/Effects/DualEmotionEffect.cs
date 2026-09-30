using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // A Bard effect: plays joy or sadness, whichever the Bard is feeling
    // when it lands. Gated on the fraction of its step's codes cleared.
    [Serializable]
    public class DualEmotionEffect : InputSetStepEffect
    {
        [Tooltip("Fraction of the step's codes (the whole run's, on the finale) that must be cleared.")]
        [Range(0f, 1f)]
        public float requiredClearRate = 1f;

        [SerializeReference]
        [SubclassPicker]
        public EmotionOutcome joy;

        [SerializeReference]
        [SubclassPicker]
        public EmotionOutcome sadness;

        public override bool IsTriggered(InputSetCompletionReport report)
        {
            return report.TotalSets > 0 && (float)report.ClearedSets / report.TotalSets >= requiredClearRate;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            var outcome = MoodSwingPassive.EmotionOf(context.Self) == Emotion.Joy ? joy : sadness;
            outcome?.Apply(in context);
        }
    }
}
