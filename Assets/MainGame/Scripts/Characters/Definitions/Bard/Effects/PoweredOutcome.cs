using System;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // Powers up its targets: their skills deal more damage for a while.
    [Serializable]
    public class PoweredOutcome : EmotionOutcome
    {
        [Min(0f)]
        public float damageIncrease = 0.5f;

        [Tooltip("Turns of the target's own; 2 lasts through its next turn (statuses tick at turn start).")]
        [Min(1)]
        public int turns = 2;

        public OutcomeTargets targets = OutcomeTargets.CardTargets;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in TargetsOf(in context, targets))
            {
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Powered, turns, "bard") { outgoingDamageMultiplier = 1f + damageIncrease });
            }
        }
    }
}
