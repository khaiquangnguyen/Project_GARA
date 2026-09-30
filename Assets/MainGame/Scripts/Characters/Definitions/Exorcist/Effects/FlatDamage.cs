using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Flat damage to every target, when its gate passes.
    [Serializable]
    public class FlatDamage : GatedRhythmStepEffect
    {
        [Min(1)]
        public int damage = 2;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
