using GARA.Characters;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using Event = Spine.Event;

namespace GARA.Combat
{
    // Shared "play one named Spine clip" behavior. Concrete leaf subclasses
    // (Atk1State, IdleState, ...) exist only so each is its own distinct
    // type — CharacterState resolution assumes at most one instance of a
    // given concrete type per prefab, so every state needs a real subclass
    // even when the behavior is identical. States may live on a child of
    // the character's root (commonly under "States"), and the Spine visual
    // lives on its own child too (commonly "Visual") — neither is an
    // ancestor of the other, so the skeleton is found by walking up to the
    // prefab's root (CharacterDefinition always lives there) and back down.
    public abstract class SpineAnimationState : CharacterState
    {
        // Purely to give the [SpineAnimation] dropdown below a data source —
        // Spine's own attribute drawer only falls back to GetComponent/
        // GetComponentInChildren when dataField is empty, which can't reach
        // a SkeletonRenderer sitting on a parent (e.g. when states live
        // under a "States" child). Assign once per state when authoring
        // the prefab (already done on the ones that exist today).
        [SerializeField] private SkeletonRenderer skeletonRendererRef;

        [FormerlySerializedAs("_animationName")]
        [SpineAnimation(dataField: nameof(skeletonRendererRef))]
        [SerializeField] private string animationName;
        [FormerlySerializedAs("_loop")]
        [SerializeField] private bool loop;

        private SkeletonAnimation _skeletonAnimation;
        private TrackEntry _activeTrackEntry;

        public override void Enter(CharacterStateContext context)
        {
            _activeTrackEntry = PlayAnimation();
            _activeTrackEntry.Complete += OnAnimationComplete;
            _activeTrackEntry.Event += OnTrackEvent;
        }

        public override void Exit()
        {
            if (_activeTrackEntry != null)
            {
                _activeTrackEntry.Complete -= OnAnimationComplete;
                _activeTrackEntry.Event -= OnTrackEvent;
                _activeTrackEntry = null;
            }
        }

        // Editor-only authoring aid: plays the clip without going through
        // the combat Enter/Exit lifecycle — no context, no Finished event.
        public void PreviewAnimation()
        {
            PlayAnimation();
        }

        private TrackEntry PlayAnimation()
        {
            if (_skeletonAnimation == null)
            {
                var characterRoot = GetComponentInParent<CharacterDefinition>();
                _skeletonAnimation = characterRoot != null
                    ? characterRoot.GetComponentInChildren<SkeletonAnimation>()
                    : GetComponentInParent<SkeletonAnimation>(); // fallback if authored without a CharacterDefinition
            }

            return _skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        }

        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnAnimationComplete;
            _activeTrackEntry = null;
            RaiseFinished();
        }

        private void OnTrackEvent(TrackEntry trackEntry, Event e)
        {
            Debug.Log($"[SpineAnimationState] {name} ({animationName}) fired event '{e.Data.Name}'");
            OnSpineEvent(e.Data.Name);
        }

        // Hook for subclasses to react to named Spine animation events —
        // user events authored directly on the clip in Spine (e.g. a "Hit"
        // event marking the actual hit-frame, rather than firing on Enter
        // regardless of where in the animation that lands). No-op by
        // default.
        protected virtual void OnSpineEvent(string eventName)
        {
        }
    }
}
