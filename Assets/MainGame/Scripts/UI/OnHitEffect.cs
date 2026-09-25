using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;
using GARA.Combat;

// Plain component — deliberately not part of the GARA.Combat assembly or
// namespace, mirroring OnNotTargetedEffect. Declares, in exactly one place,
// what happens to a character the instant it takes damage. Combat code
// never touches this directly — it only broadcasts HitStateEvent via
// MMEventManager (see CombatParticipant.ApplyDamage); this class listens
// for that and plays its own MMF_Player. Swapping the effect — a different
// feedback, more feedbacks, a completely different reaction — never
// touches combat code.
//
// One instance of this lives on its own child of the EffectExampleDummy
// prefab, alongside OnNotTargetedEffect. At scene start, CombatSceneManager
// clones that child onto every character in the scene; each clone retargets
// itself to its new owner on Awake (see RetargetToOwner) rather than
// needing any external wiring call.
public class OnHitEffect : MonoBehaviour, MMEventListener<HitStateEvent>
{
    [Tooltip("The feedback(s) to play the instant this character takes damage.")]
    [SerializeField] private MMF_Player feedback;

    private GameObject _owner;

    private void Awake()
    {
        RetargetToOwner();
    }

    // Self-retargets to whichever character this clone was parented under
    // — no external call needed. Assumes it's a direct child of the
    // character's SceneRoot (see CombatSceneManager's cloning step). Also
    // makes sure that character's Spine visual has an MMPositionShaker on
    // it (nothing ships one by default) and points any MMF_PositionShake
    // feedback's TargetShaker directly at it — left blank, that feedback
    // broadcasts on a channel instead, which every character's shaker would
    // answer to at once rather than just this one.
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
            if (f is MMF_PositionShake positionShakeFeedback)
            {
                positionShakeFeedback.TargetShaker = shaker;
            }
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening<HitStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<HitStateEvent>();
    }

    public void OnMMEvent(HitStateEvent hitStateEvent)
    {
        if (_owner == null || hitStateEvent.sceneRoot != _owner)
        {
            return;
        }

        OnTrigger();
    }

    // Exposed directly (not just reachable through the MM event) so a
    // preview tool — see EffectDummy.Preview — can trigger the exact same
    // effect on an arbitrary character without needing the combat system
    // running.
    public void OnTrigger()
    {
        feedback?.PlayFeedbacks();
    }

    // Resets to the feedbacks' initial values — see OnNotTargetedEffect for
    // why RestoreInitialValues(), not ResetFeedbacks(), is the right call.
    public void OnDone()
    {
        feedback?.RestoreInitialValues();
    }
}
