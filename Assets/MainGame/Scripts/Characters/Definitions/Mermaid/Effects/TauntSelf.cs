using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // The Mermaid taunts: opponents' target-picking cards must pick her
    // while it lasts (see TargetRestrictions).
    [Serializable]
    public class TauntSelf : GatedRhythmStepEffect
    {
        [Tooltip("Turns (her own) it lasts.")]
        [Min(1)]
        public int turns = 1;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            context.Self?.ApplyStatus(StatusEffectInstance.Taunt(turns, "taunt"));
        }
    }
}
