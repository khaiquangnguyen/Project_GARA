using System;
using GARA.SkillCards.Rhythm;

namespace GARA.Characters.Mermaid
{
    // Turns every ally among the targets, except the Mermaid herself,
    // invisible until they next act.
    [Serializable]
    public class MakeAlliesInvisible : GatedRhythmStepEffect
    {
        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                if (target != context.Self && !target.IsDefeated && target.Faction == context.Self.Faction && !target.HasStatus(StatusEffectKind.Invisible))
                {
                    target.ApplyStatus(StatusEffectInstance.Invisible("star singer"));
                }
            }
        }
    }
}
