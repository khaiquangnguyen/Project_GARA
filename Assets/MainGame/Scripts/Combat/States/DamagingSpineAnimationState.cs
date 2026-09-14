using System.Collections.Generic;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Shared base for any Spine-animated action that deals flat damage to
    // every target it was given (a basic attack, a combo finisher, a
    // special). Damage is applied on the clip's "hit" user event (matched
    // case-insensitively by lowercasing before comparing) rather than
    // immediately on Enter — the event must be authored on the animation in
    // Spine itself, at whatever frame the swing actually connects. A clip
    // with no such event never deals damage.
    public abstract class DamagingSpineAnimationState : SpineAnimationState
    {
        private const string HitEventName = "hit";

        [SerializeField] private int damageAmount;

        private IReadOnlyList<ICombatTarget> _pendingTargets;

        public override void Enter(CharacterStateContext context)
        {
            base.Enter(context);
            _pendingTargets = context.Targets;
        }

        protected override void OnSpineEvent(string eventName)
        {
            if (_pendingTargets == null || eventName == null || eventName.ToLowerInvariant() != HitEventName)
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
