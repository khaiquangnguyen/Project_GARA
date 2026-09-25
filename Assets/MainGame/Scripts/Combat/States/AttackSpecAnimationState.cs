using GARA.Characters;

namespace GARA.Combat
{
    // Shared base for a state that plays an AttackAnimationSpec: a default
    // spec from the subclass, a one-play override, and the spec's range and
    // "hit" event timing for staging.
    public abstract class AttackSpecAnimationState : SpineAnimationState
    {
        protected const string HitEventName = "hit";

        private AttackAnimationSpec _nextSpec;

        // The spec the next Enter will play — the override if one is set.
        public AttackAnimationSpec NextSpec => _nextSpec != null ? _nextSpec : DefaultSpec;

        protected abstract AttackAnimationSpec DefaultSpec { get; }

        protected override string ClipName => DefaultSpec != null ? DefaultSpec.AnimationName : null;

        // Plays this spec instead of DefaultSpec on the next Enter only.
        public void OverrideNextSpec(AttackAnimationSpec spec)
        {
            _nextSpec = spec;
            OverrideNextClip(spec != null ? spec.AnimationName : null);
        }

        // Seconds from the next Enter to the next spec's hit event.
        public bool TryGetSecondsToHit(out float seconds)
        {
            seconds = 0f;
            return NextSpec != null && TryGetEventSeconds(NextSpec.AnimationName, HitEventName, out seconds);
        }

        public override void Enter(CharacterStateContext context)
        {
            base.Enter(context);
            _nextSpec = null;
        }

        protected static bool IsHitEvent(string eventName)
        {
            return eventName != null && eventName.ToLowerInvariant() == HitEventName;
        }
    }
}
