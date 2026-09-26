using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.FrogBard
{
    // A Frog Bard effect: plays joy or sadness, whichever the Bard is feeling
    // when it lands. Gated on the fraction of its step's notes hit.
    [Serializable]
    public class DualEmotionEffect : RhythmStepEffect
    {
        [Tooltip("Fraction of the bar's notes (the whole run, on the finale) that must be hit.")]
        [Range(0f, 1f)]
        public float requiredHitRate = 1f;

        [SerializeReference]
        [SubclassPicker]
        public EmotionOutcome joy;

        [SerializeReference]
        [SubclassPicker]
        public EmotionOutcome sadness;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.TotalNotes > 0 && (float)report.HitNotes / report.TotalNotes >= requiredHitRate;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            var outcome = MoodSwingPassive.EmotionOf(context.Self) == Emotion.Joy ? joy : sadness;
            outcome?.Apply(in context);
        }
    }
}
