using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Seals every target directly (see RiteOfExorcismPassive), when its gate
    // passes.
    [Serializable]
    public class AddSealsEffect : GatedRhythmStepEffect
    {
        [Min(1)]
        public int seals = 1;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                RiteOfExorcismPassive.AddSeals(context.Self, target, seals);
            }
        }
    }
}
