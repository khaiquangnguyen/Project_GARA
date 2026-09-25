using GARA.Combat;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;

// Root of an attack's on-hit feedback prefab, referenced by an
// AttackAnimationSpec or a card's finale and spawned under the attacker.
public class AttackHitFeedback : MonoBehaviour, IAttackHitFeedback
{
    [Tooltip("The feedback(s) to play on the attack's hit.")]
    [SerializeField] private MMF_Player feedback;

    private void Awake()
    {
        RetargetToOwner();
    }

    // Same retargeting as OnHitEffect.RetargetToOwner.
    private void RetargetToOwner()
    {
        if (transform.parent == null)
        {
            return;
        }

        var skeletonRenderer = transform.parent.GetComponentInChildren<SkeletonRenderer>();
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

    public void Play()
    {
        feedback?.PlayFeedbacks();
    }
}
