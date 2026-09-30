using System;
using GARA.InputSets;
using GARA.SkillCards.InputSets;

namespace GARA.Characters.Bard
{
    // Changes the Bard's mood when it lands; only when every code was
    // cleared. Put it after the DualEmotionEffects it follows, so they play
    // the old mood.
    [Serializable]
    public class MoodChangeEffect : InputSetStepEffect
    {
        public MoodChange change = MoodChange.Swing;

        public override bool IsTriggered(InputSetCompletionReport report)
        {
            return !report.WasAborted && report.TotalSets > 0 && report.ClearedSets == report.TotalSets;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            MoodSwingPassive.ChangeMood(context.Self, change);
        }
    }
}
