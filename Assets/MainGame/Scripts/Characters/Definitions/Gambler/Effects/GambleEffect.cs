using System;
using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // An effect on a Gambler card's move, gated on the roll.
    [Serializable]
    public abstract class GambleEffect : ISkillEffect
    {
        // The card warns when its game rolls a different outcome type.
        public virtual Type OutcomeType => typeof(GambleOutcome);

        public abstract bool IsTriggered(GambleOutcome outcome, SkillPerformance performance);

        public abstract void ApplyEffect(in SkillEffectContext context);

        protected static GambleOutcome OutcomeOf(in SkillEffectContext context)
        {
            return context.Performance.TryGetDetails<GambleReport>(out var report) ? report.Outcome : null;
        }
    }

    // A GambleEffect for one game's outcome; never triggers on another's.
    [Serializable]
    public abstract class GambleEffect<TOutcome> : GambleEffect where TOutcome : GambleOutcome
    {
        public override Type OutcomeType => typeof(TOutcome);

        public sealed override bool IsTriggered(GambleOutcome outcome, SkillPerformance performance)
        {
            return outcome is TOutcome typed && IsTriggered(typed, performance);
        }

        public sealed override void ApplyEffect(in SkillEffectContext context)
        {
            if (OutcomeOf(context) is TOutcome typed)
            {
                ApplyEffect(context, typed);
            }
        }

        protected abstract bool IsTriggered(TOutcome outcome, SkillPerformance performance);

        protected abstract void ApplyEffect(in SkillEffectContext context, TOutcome outcome);
    }
}
