using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;
using GARA.Combat;

// Plays its feedback when its owner parries a hit (ParrySuccessStateEvent).
// Cloned onto each character from the effect dummy, like OnHitEffect.
public class OnParrySuccessEffect : MonoBehaviour, MMEventListener<ParrySuccessStateEvent>
{
    [Tooltip("The feedback(s) to play the instant this character parries a hit.")]
    [SerializeField] private MMF_Player feedback;

    private GameObject _owner;

    private void Awake()
    {
        RetargetToOwner();
    }

    // Same retargeting as OnHitEffect.RetargetToOwner.
    private void RetargetToOwner()
    {
        if (transform.parent == null)
        {
            return; // still the dummy's own authoring-time child — nothing to retarget
        }

        _owner = transform.parent.gameObject;

        var skeletonRenderer = _owner.GetComponentInChildren<SkeletonRenderer>();
        if (skeletonRenderer == null || feedback == null)
        {
            return;
        }

        var shaker = skeletonRenderer.GetComponent<MMPositionShaker>();
        if (shaker == null)
        {
            shaker = skeletonRenderer.gameObject.AddComponent<MMPositionShaker>();
        }

        foreach (var f in feedback.FeedbacksList)
        {
            if (f is MMF_PositionShake positionShakeFeedback && positionShakeFeedback.TargetShaker != null)
            {
                positionShakeFeedback.TargetShaker = shaker;
            }
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening<ParrySuccessStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<ParrySuccessStateEvent>();
    }

    public void OnMMEvent(ParrySuccessStateEvent parrySuccessStateEvent)
    {
        if (_owner == null || parrySuccessStateEvent.sceneRoot != _owner)
        {
            return;
        }

        OnTrigger();
    }

    public void OnTrigger()
    {
        feedback?.PlayFeedbacks();
    }

    public void OnDone()
    {
        feedback?.RestoreInitialValues();
    }
}
