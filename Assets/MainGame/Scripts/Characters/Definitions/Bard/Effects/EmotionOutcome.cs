using System;

namespace GARA.Characters.Bard
{
    // What one side (joy or sadness) of a DualEmotionEffect does.
    [Serializable]
    public abstract class EmotionOutcome
    {
        public abstract void Apply(in SkillEffectContext context);
    }
}
