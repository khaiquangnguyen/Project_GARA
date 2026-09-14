using System.Collections;
using Spine.Unity;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    // A small custom Feel feedback, modeled directly on the built-in
    // MMF_SpriteRendererAlpha, for the one thing that's missing from
    // MMFeedbacks out of the box: tweening a Spine SkeletonAnimation's
    // alpha. Supports normal and reverse playback (via the base class's
    // NormalPlayDirection) so one authored feedback can both dim and
    // restore a character — see OnNotTargetedEffect, which calls
    // PlayFeedbacks()/PlayFeedbacksInReverse() on the MMF_Player that owns
    // an instance of this feedback.
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you change the alpha of a target Spine SkeletonAnimation over time, tweening towards a destination alpha (and back, when played in reverse).")]
    [System.Serializable]
    [FeedbackPath("Renderer/Skeleton Animation Alpha")]
    public class MMF_SkeletonAlpha : MMF_Feedback
    {
        public static bool FeedbackTypeAuthorized = true;
#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.RendererColor;
        public override bool EvaluateRequiresSetup() => BoundSkeletonAnimation == null;
        public override string RequiredTargetText => BoundSkeletonAnimation != null ? BoundSkeletonAnimation.name : "";
        public override string RequiresSetupText => "This feedback requires that a BoundSkeletonAnimation be set to be able to work properly. You can set one below.";
#endif

        public override float FeedbackDuration { get => ApplyTimeMultiplier(Duration); set => Duration = value; }
        public override bool HasChannel => true;
        public override bool HasRandomness => true;
        public override bool HasAutomatedTargetAcquisition => true;
        protected override void AutomateTargetAcquisition() => BoundSkeletonAnimation = FindAutomatedTarget<SkeletonAnimation>();

        [MMFInspectorGroup("Skeleton Animation", true, 51, true)]
        [Tooltip("the SkeletonAnimation to affect when playing the feedback")]
        public SkeletonAnimation BoundSkeletonAnimation;

        [Tooltip("how long the alpha should change over time")]
        public float Duration = 0.15f;

        [Tooltip("the alpha to move towards when playing this feedback forward")]
        public float DestinationAlpha = 0.2f;

        [Tooltip("the curve to tween alpha on")]
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));

        [Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")]
        public bool AllowAdditivePlays = false;

        protected Coroutine _coroutine;
        protected float _initialAlpha;
        protected float _defaultAlpha = 1f;
        protected bool _defaultAlphaCaptured;

        // Captured once, the first time this feedback is used (on init if
        // possible, else lazily on the first play) — the true "back to
        // normal" alpha, as opposed to _initialAlpha, which is re-captured
        // on every play so a fresh play tweens from wherever alpha
        // currently sits (e.g. re-triggering the dim mid-fade). Restoring
        // via _initialAlpha alone breaks the very first time
        // CustomRestoreInitialValues (i.e. ResetFeedbacks) runs before this
        // feedback has ever played — _initialAlpha still holds its C#
        // default of 0, so the restore would snap alpha to fully
        // transparent instead of back to normal.
        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);
            CaptureDefaultAlpha();
        }

        protected virtual void CaptureDefaultAlpha()
        {
            if (_defaultAlphaCaptured || BoundSkeletonAnimation == null)
            {
                return;
            }

            _defaultAlpha = BoundSkeletonAnimation.Skeleton.GetColor().a;
            _defaultAlphaCaptured = true;
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || !FeedbackTypeAuthorized || BoundSkeletonAnimation == null)
            {
                return;
            }

            CaptureDefaultAlpha();
            _initialAlpha = BoundSkeletonAnimation.Skeleton.GetColor().a;

            if (!AllowAdditivePlays && _coroutine != null)
            {
                return;
            }

            if (_coroutine != null)
            {
                Owner.StopCoroutine(_coroutine);
            }

            _coroutine = Owner.StartCoroutine(AlphaSequence(feedbacksIntensity, position));
        }

        protected virtual IEnumerator AlphaSequence(float intensity, Vector3 position)
        {
            var journey = NormalPlayDirection ? 0f : FeedbackDuration;
            IsPlaying = true;

            while (journey >= 0 && journey <= FeedbackDuration && FeedbackDuration > 0)
            {
                var remapped = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);
                var curveValue = Curve.Evaluate(remapped);
                var newAlpha = Mathf.Lerp(_initialAlpha, DestinationAlpha, curveValue);
                SetAlpha(newAlpha, intensity, position);

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                yield return null;
            }

            SetAlpha(NormalPlayDirection ? DestinationAlpha : _initialAlpha, intensity, position);
            _coroutine = null;
            IsPlaying = false;
        }

        protected virtual void SetAlpha(float newAlpha, float feedbacksIntensity, Vector3 position)
        {
            var intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            var setAlpha = Mathf.Clamp01(newAlpha * intensityMultiplier);
            var color = BoundSkeletonAnimation.Skeleton.GetColor();
            BoundSkeletonAnimation.Skeleton.SetColor(color.r, color.g, color.b, setAlpha);
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || !FeedbackTypeAuthorized || _coroutine == null)
            {
                return;
            }

            base.CustomStopFeedback(position, feedbacksIntensity);
            Owner.StopCoroutine(_coroutine);
            IsPlaying = false;
            _coroutine = null;
        }

        protected override void CustomRestoreInitialValues()
        {
            if (!Active || !FeedbackTypeAuthorized || BoundSkeletonAnimation == null)
            {
                return;
            }

            CaptureDefaultAlpha();
            SetAlpha(_defaultAlpha, 1f, Owner.transform.position);
        }

        public override void OnDisable()
        {
            _coroutine = null;
        }
    }
}
