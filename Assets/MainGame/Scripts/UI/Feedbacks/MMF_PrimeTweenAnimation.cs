using PrimeTween;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    // Bridges Feel and PrimeTween: drives a PrimeTween TweenAnimationComponent
    // (an Inspector-authored tween sequence) from an MMF_Player, so tweens can
    // be authored in PrimeTween and sequenced alongside any other feedback.
    // Reports the tween's live total duration while it's playing, so a
    // following MMF_HoldingPause waits for it to finish — e.g. a note's
    // hit feedback can tween, hold, then destroy the note.
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback plays a PrimeTween TweenAnimationComponent. Pick the action to perform on it when this feedback plays. While the tween is playing, this feedback's duration matches the tween's, so a Holding Pause placed after it waits for the tween to finish (infinite tweens report 0).")]
    [System.Serializable]
    [FeedbackPath("Animation/PrimeTween Animation")]
    public class MMF_PrimeTweenAnimation : MMF_Feedback
    {
        public enum Actions
        {
            Trigger,
            SetStateTrue,
            SetStateFalse,
            ToggleState,
            Stop,
            Complete,
            Reset
        }

        public static bool FeedbackTypeAuthorized = true;
#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.AnimationColor;
        public override bool EvaluateRequiresSetup() => TargetAnimation == null;
        public override string RequiredTargetText => TargetAnimation != null ? TargetAnimation.name + " (" + Action + ")" : "";
        public override string RequiresSetupText => "This feedback requires that a TargetAnimation be set to be able to work properly. You can set one below.";
#endif

        public override float FeedbackDuration
        {
            get
            {
                if (TargetAnimation == null || TargetAnimation.animation == null || !TargetAnimation.animation.isAlive)
                {
                    return 0f;
                }

                var total = TargetAnimation.animation.durationTotal;
                return float.IsInfinity(total) ? 0f : total;
            }
        }

        public override bool HasAutomatedTargetAcquisition => true;
        protected override void AutomateTargetAcquisition() => TargetAnimation = FindAutomatedTarget<TweenAnimationComponent>();

        [MMFInspectorGroup("PrimeTween Animation", true, 51, true)]
        [Tooltip("the PrimeTween TweenAnimationComponent to control")]
        public TweenAnimationComponent TargetAnimation;

        [Tooltip("what to do to the animation when this feedback plays. Trigger plays simple animations from the start and flips the direction of reversible ones; SetStateTrue/False play a reversible animation to its end/start")]
        public Actions Action = Actions.Trigger;

        [Tooltip("if this is true, stopping this feedback (or its player) also stops the tween")]
        public bool StopTweenOnStop = true;

        [Tooltip("if this is true, restoring this feedback's initial values (e.g. MMF_Player.RestoreInitialValues) resets the tween to its beginning")]
        public bool ResetTweenOnRestore = true;

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || !FeedbackTypeAuthorized || TargetAnimation == null)
            {
                return;
            }

            var animation = TargetAnimation.animation;
            switch (Action)
            {
                case Actions.Trigger:
                    animation.Trigger();
                    break;
                case Actions.SetStateTrue:
                    animation.state = true;
                    break;
                case Actions.SetStateFalse:
                    animation.state = false;
                    break;
                case Actions.ToggleState:
                    animation.ToggleState();
                    break;
                case Actions.Stop:
                    animation.Stop();
                    break;
                case Actions.Complete:
                    animation.Complete();
                    break;
                case Actions.Reset:
                    animation.Reset();
                    break;
            }
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || !FeedbackTypeAuthorized || !StopTweenOnStop || TargetAnimation == null)
            {
                return;
            }

            base.CustomStopFeedback(position, feedbacksIntensity);
            TargetAnimation.animation.Stop();
        }

        protected override void CustomRestoreInitialValues()
        {
            if (!Active || !FeedbackTypeAuthorized || !ResetTweenOnRestore || TargetAnimation == null)
            {
                return;
            }

            TargetAnimation.animation.Reset();
        }
    }
}
