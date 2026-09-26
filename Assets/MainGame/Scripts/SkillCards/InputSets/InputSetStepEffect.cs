using System;
using GARA.Characters;
using GARA.InputSets;

namespace GARA.SkillCards.InputSets
{
    // An effect on a live input-set card's step, with its gate.
    [Serializable]
    public abstract class InputSetStepEffect : ISkillEffect
    {
        public abstract bool IsTriggered(InputSetCompletionReport report);

        public abstract void ApplyEffect(in SkillEffectContext context);
    }
}
