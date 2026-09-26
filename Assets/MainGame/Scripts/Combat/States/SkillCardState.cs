using GARA.Characters;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Combat
{
    // A skill card's state on its character: plays the card's own
    // animationSpec (a live card's perfect finale, and the range it dashes in
    // to) unless the executor overrides it for one play (a live bar's move).
    // Deals no damage itself — the card's effects resolve through
    // CharacterStateContext.OnImpact on the clip's "hit" event.
    //
    // Safety net: if the hit event never fires (missing on the clip, or the
    // state is cut short, e.g. by the next live bar), OnImpact is still
    // invoked once, so a card's effects are never silently dropped.
    public abstract class SkillCardState : AttackSpecAnimationState
    {
        [Tooltip("The skill card that plays this state. The character's card pool is built from these.")]
        [Expandable]
        [SerializeField]
        private SkillCardDefinition skillCard;

        public override SkillCardDefinition SkillCard => skillCard;

        protected override AttackAnimationSpec DefaultSpec => skillCard != null ? skillCard.animationSpec : null;

        private CharacterStateContext _context;
        private bool _impactInvoked;
        private bool _impactOnEveryHit;

        public override void Enter(CharacterStateContext context)
        {
            // Read before base.Enter consumes the one-play override.
            _impactOnEveryHit = (skillCard != null && skillCard.ImpactOnEveryHit)
                                || (NextSpec != null && NextSpec.ImpactOnEveryHit);
            base.Enter(context);
            _context = context;
            _impactInvoked = false;
        }

        protected override void OnSpineEvent(string eventName)
        {
            if (!IsHitEvent(eventName))
            {
                return;
            }

            if (_impactOnEveryHit)
            {
                _impactInvoked = true;
                _context.OnImpact?.Invoke();
                return;
            }

            InvokeImpactIfNeeded();
        }

        protected override void OnBeforeFinished()
        {
            InvokeImpactIfNeeded();
        }

        // Cut short (e.g. by the next live bar) before its hit event — still
        // deliver the impact.
        public override void Exit()
        {
            InvokeImpactIfNeeded();
            base.Exit();
        }

        private void InvokeImpactIfNeeded()
        {
            if (_impactInvoked)
            {
                return;
            }

            _impactInvoked = true;
            _context.OnImpact?.Invoke();
        }
    }
}
