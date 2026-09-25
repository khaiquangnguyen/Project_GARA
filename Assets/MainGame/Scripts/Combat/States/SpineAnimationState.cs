using System;
using GARA.Characters;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

namespace GARA.Combat
{
    // Shared "play one Spine clip" behavior; subclasses say which clip
    // (NamedSpineAnimationState by name, AttackSpecAnimationState through
    // an AttackAnimationSpec). Concrete leaf subclasses
    // (DancerDivineState, IdleState, ...) exist only so each is its own distinct
    // type — CharacterState resolution assumes at most one instance of a
    // given concrete type per prefab, so every state needs a real subclass
    // even when the behavior is identical. States may live on a child of
    // the character's root (commonly under "States"), and the Spine visual
    // lives on its own child too (commonly "Visual") — neither is an
    // ancestor of the other, so the skeleton is found by walking up to the
    // prefab's root (CharacterDefinition always lives there) and back down.
    public abstract class SpineAnimationState : CharacterState
    {
        private SkeletonAnimation _skeletonAnimation;
        private TrackEntry _activeTrackEntry;
        private string _nextClip;

        protected abstract string ClipName { get; }

        protected virtual bool Loop => false;

        // Plays this clip instead of ClipName on the next Enter only.
        protected void OverrideNextClip(string clip)
        {
            _nextClip = clip;
        }

        public override void Enter(CharacterStateContext context)
        {
            _activeTrackEntry = PlayAnimation(_nextClip ?? ClipName);
            _nextClip = null;
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
            PlayAnimation(ClipName);
        }

        private SkeletonAnimation Skeleton
        {
            get
            {
                if (_skeletonAnimation == null)
                {
                    var characterRoot = GetComponentInParent<CharacterDefinition>();
                    _skeletonAnimation = characterRoot != null
                        ? characterRoot.GetComponentInChildren<SkeletonAnimation>()
                        : GetComponentInParent<SkeletonAnimation>(); // fallback if authored without a CharacterDefinition
                }

                return _skeletonAnimation;
            }
        }

        private TrackEntry PlayAnimation(string clip)
        {
            return Skeleton.AnimationState.SetAnimation(0, clip, Loop);
        }

        // Seconds from the clip's start to its first eventName user event, at
        // the skeleton's current time scale. False if the clip has none.
        protected bool TryGetEventSeconds(string clip, string eventName, out float seconds)
        {
            seconds = 0f;
            var skeleton = Skeleton;
            var animation = skeleton != null && clip != null ? skeleton.Skeleton.Data.FindAnimation(clip) : null;
            if (animation == null)
            {
                return false;
            }

            foreach (var timeline in animation.Timelines)
            {
                if (!(timeline is EventTimeline eventTimeline))
                {
                    continue;
                }

                foreach (var e in eventTimeline.Events)
                {
                    if (string.Equals(e.Data.Name, eventName, StringComparison.OrdinalIgnoreCase))
                    {
                        seconds = skeleton.timeScale > 0f ? e.Time / skeleton.timeScale : e.Time;
                        return true;
                    }
                }
            }

            return false;
        }

        // False for states timed by something other than the clip (see
        // JumpState).
        protected virtual bool FinishesOnAnimationComplete => true;

        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            if (!FinishesOnAnimationComplete)
            {
                return;
            }

            trackEntry.Complete -= OnAnimationComplete;
            _activeTrackEntry = null;
            OnBeforeFinished();
            RaiseFinished();
        }

        // Hook for a subclass that needs to guarantee some side effect has
        // happened by the time Finished fires, even if the animation
        // completed without ever raising the Spine event that would
        // normally trigger it (see SkillCardState). No-op by
        // default.
        protected virtual void OnBeforeFinished()
        {
        }

        private void OnTrackEvent(TrackEntry trackEntry, Event e)
        {
            Debug.Log($"[SpineAnimationState] {name} ({ClipName}) fired event '{e.Data.Name}'");
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
