using GARA.Characters;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Combat
{
    // Shared "play one named Spine clip" behavior. Concrete leaf subclasses
    // (Atk1State, IdleState, ...) exist only so each is its own distinct
    // type — CharacterState resolution assumes at most one instance of a
    // given concrete type per prefab, so every state needs a real subclass
    // even when the behavior is identical. States may live on a child of
    // the character's root (not necessarily the same GameObject as the
    // Spine components), so the skeleton is found via GetComponentInParent.
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
        }

        public override void Exit()
        {
            if (_activeTrackEntry != null)
            {
                _activeTrackEntry.Complete -= OnAnimationComplete;
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
                _skeletonAnimation = GetComponentInParent<SkeletonAnimation>();
            }

            return _skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        }

        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnAnimationComplete;
            _activeTrackEntry = null;
            RaiseFinished();
        }
    }
}
