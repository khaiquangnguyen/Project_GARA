using GARA.Characters;

namespace GARA.Combat
{
    // Shared base for a skill card's own animation. Derives from
    // DamagingSpineAnimationState purely to reuse its Spine "hit" user-event
    // wiring (matching the event by name, case-insensitively) — NOT its
    // damage application. A skill card's damage/other effects are authored
    // on the card itself (SkillCardDefinition.effects, e.g.
    // DamageSkillEffect) and resolved by CombatSceneManager through
    // CharacterStateContext.OnImpact once the card's input minigame has
    // resolved. If this class also let DamagingSpineAnimationState apply
    // its own flat damageAmount off the same hit event, a card using
    // DamageSkillEffect would deal damage twice. So OnSpineEvent below
    // deliberately does NOT call base.OnSpineEvent — damageAmount on a
    // skill-card leaf prefab is inert; authors should leave it at 0 and
    // drive damage through DamageSkillEffect instead.
    //
    // Safety net: if the clip's hit event never fires (missing/misauthored
    // on the clip), OnImpact is still invoked once, right before Finished
    // (via SpineAnimationState.OnBeforeFinished), so a card's effects are
    // never silently dropped.
    public abstract class SkillCardAnimationState : DamagingSpineAnimationState
    {
        private const string HitEventName = "hit";

        private CharacterStateContext _context;
        private bool _impactInvoked;

        public override void Enter(CharacterStateContext context)
        {
            base.Enter(context);
            _context = context;
            _impactInvoked = false;
        }

        protected override void OnSpineEvent(string eventName)
        {
            if (eventName == null || eventName.ToLowerInvariant() != HitEventName)
            {
                return;
            }

            InvokeImpactIfNeeded();
        }

        protected override void OnBeforeFinished()
        {
            InvokeImpactIfNeeded();
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
