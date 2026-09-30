using System;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // Weakens its targets: their skills deal less damage for a while.
    [Serializable]
    public class WeakenedOutcome : EmotionOutcome
    {
        [Range(0f, 1f)]
        public float damageReduction = 0.25f;

        [Tooltip("Turns of the target's own; 2 lasts through its next turn (statuses tick at turn start).")]
        [Min(1)]
        public int turns = 2;

        public OutcomeTargets targets = OutcomeTargets.CardTargets;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in TargetsOf(in context, targets))
            {
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Weakened, turns, "bard") { outgoingDamageMultiplier = 1f - damageReduction });
            }
        }
    }
}
