using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;
using GARA.Combat;

// Plays its feedback when its owner evades a hit (an evasion stack is used up)
// (EvadeSuccessStateEvent).
// Cloned onto each character from the effect dummy, like OnHitEffect.
public class OnEvadeSuccessEffect : MonoBehaviour, MMEventListener<EvadeSuccessStateEvent>
{
    [Tooltip("The feedback(s) to play the instant this character evades a hit (an evasion stack is used up).")]
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
        this.MMEventStartListening<EvadeSuccessStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<EvadeSuccessStateEvent>();
    }

    public void OnMMEvent(EvadeSuccessStateEvent evadeSuccessStateEvent)
    {
        if (_owner == null || evadeSuccessStateEvent.sceneRoot != _owner)
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
