using System.Collections.Generic;
using GARA.Characters;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Combat
{
    // Shared base for any Spine-animated action that deals flat damage to
    // every target it was given (a basic attack, a combo finisher). Damage is
    // applied on the clip's "hit" user event (matched case-insensitively)
    // rather than immediately on Enter — the event must be authored on the
    // animation in Spine itself, at whatever frame the swing actually
    // connects. A clip with no such event never deals damage. The clip (and
    // its staging data, e.g. range) comes from animationSpec.
    public abstract class DamagingSpineAnimationState : AttackSpecAnimationState
    {
        [Expandable]
        [SerializeField]
        private AttackAnimationSpec animationSpec;

        [SerializeField]
        private int damageAmount;

        private IReadOnlyList<ICombatTarget> _pendingTargets;

        public AttackAnimationSpec AnimationSpec => animationSpec;

        protected override AttackAnimationSpec DefaultSpec => animationSpec;

        public override void Enter(CharacterStateContext context)
        {
            base.Enter(context);
            _pendingTargets = context.Targets;
        }

        protected override void OnSpineEvent(string eventName)
        {
            if (_pendingTargets == null || !IsHitEvent(eventName))
            {
                return;
            }

            foreach (var target in _pendingTargets)
            {
                target.ApplyDamage(damageAmount);
            }
        }
    }
}
