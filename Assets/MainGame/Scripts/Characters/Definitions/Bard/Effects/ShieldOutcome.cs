using System;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // Shields its targets: each stack blocks one incoming hit, lasting
    // until used.
    [Serializable]
    public class ShieldOutcome : EmotionOutcome
    {
        [Min(1)]
        public int stacks = 2;

        public OutcomeTargets targets = OutcomeTargets.CardTargets;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in TargetsOf(in context, targets))
            {
                for (var i = 0; i < stacks; i++)
                {
                    var shield = StatusEffectInstance.Permanent(StatusEffectKind.Shielded, "bard");
                    shield.blocksNextHit = true;
                    target.ApplyStatus(shield);
                }
            }
        }
    }
}
