using System;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // Slows its targets: lower Speed, so their turns come later.
    [Serializable]
    public class SlowedOutcome : EmotionOutcome
    {
        [Range(0f, 1f)]
        public float speedReduction = 0.25f;

        [Tooltip("Turns of the target's own; 1 wears off as its next turn starts.")]
        [Min(1)]
        public int turns = 1;

        public OutcomeTargets targets = OutcomeTargets.CardTargets;

        public override void Apply(in SkillEffectContext context)
        {
            foreach (var target in TargetsOf(in context, targets))
            {
                target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Slowed, turns, "bard") { speedMultiplier = 1f - speedReduction });
            }
        }
    }
}
