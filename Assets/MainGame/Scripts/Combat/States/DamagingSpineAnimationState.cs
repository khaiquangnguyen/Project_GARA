using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Shared base for any Spine-animated action that deals flat damage to
    // every target it was given (a basic attack, a combo finisher, a
    // special). Damage is applied immediately on Enter — synchronously with
    // the animation starting, not on a hit-frame callback — since no
    // animation-event system exists yet.
    public abstract class DamagingSpineAnimationState : SpineAnimationState
    {
        [SerializeField] private int damageAmount;

        public override void Enter(CharacterStateContext context)
        {
            base.Enter(context);

            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damageAmount);
            }
        }
    }
}
