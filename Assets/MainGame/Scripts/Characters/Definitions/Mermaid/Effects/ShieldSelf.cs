using System;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Shields the Mermaid herself: each stack blocks one incoming hit,
    // lasting until used.
    [Serializable]
    public class ShieldSelf : GatedRhythmStepEffect
    {
        [Min(1)]
        public int stacks = 4;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            for (var i = 0; i < stacks; i++)
            {
                var shield = StatusEffectInstance.Permanent(StatusEffectKind.Shielded, "shield");
                shield.blocksNextHit = true;
                context.Self?.ApplyStatus(shield);
            }
        }
    }
}
