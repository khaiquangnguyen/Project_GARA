using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;
using GARA.Combat;

// Plain component — deliberately not part of the GARA.Combat assembly or
// namespace. Declares, in exactly one place, what happens to a character
// that isn't the selected target for the action about to play. Combat code
// never touches this directly — it only broadcasts TargetedStateEvent via
// MMEventManager (see CombatPhaseController); this class listens for that
// and decides what to do, which for now means playing (or reverse-playing)
// its own MMF_Player. Swapping the effect — a different feedback, more
// feedbacks, a completely different reaction — never touches combat code.
//
// One instance of this lives on the single child of the EffectExampleDummy
// prefab, authored against that dummy's own placeholder Visual. At scene
// start, CombatOverlayManager clones that child onto every character in
// the scene; each clone retargets itself to its new owner on Awake (see
// RetargetToOwner) rather than needing any external wiring call.
public class OnNotTargetedEffect : MonoBehaviour, MMEventListener<TargetedStateEvent>
{
    [Tooltip("This character's Spine visual. Reassigned automatically on Awake once this is cloned under a real character — only meaningful as authored on the dummy for editor preview.")]
    [SerializeField] private SkeletonAnimation spineAnimation;

    [Tooltip("The feedback(s) to play when this character is NOT the selected target, and to reverse-play when it's restored.")]
    [SerializeField] private MMF_Player feedback;

    private GameObject _owner;

    private void Awake()
    {
        RetargetToOwner();
    }

    // Self-retargets to whichever character this clone was parented under
    // — no external call needed. Assumes it's a direct child of the
    // character's SceneRoot (see CombatOverlayManager's cloning step).
    private void RetargetToOwner()
    {
        if (transform.parent == null)
        {
            return; // still the dummy's own authoring-time child — nothing to retarget
        }

        _owner = transform.parent.gameObject;
        spineAnimation = _owner.GetComponentInChildren<SkeletonAnimation>();

        if (feedback == null || spineAnimation == null)
        {
            return;
        }

        foreach (var f in feedback.FeedbacksList)
        {
            if (f is MMF_SkeletonAlpha skeletonAlphaFeedback)
            {
                skeletonAlphaFeedback.BoundSkeletonAnimation = spineAnimation;
            }
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening<TargetedStateEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<TargetedStateEvent>();
    }

    public void OnMMEvent(TargetedStateEvent targetedStateEvent)
    {
        if (_owner == null || targetedStateEvent.sceneRoot != _owner)
        {
            return;
        }

        if (targetedStateEvent.isTargeted)
        {
            OnDone();
        }
        else
        {
            OnTrigger();
        }
    }

    // Exposed directly (not just reachable through the MM event) so a
    // preview tool — see EffectDummy.Preview — can trigger the exact same
    // effect on an arbitrary character without needing the combat/targeting
    // system running.
    public void OnTrigger()
    {
        feedback?.PlayFeedbacks();
    }

    // Resets to the feedbacks' initial values rather than playing them in
    // reverse — correct for feedbacks (e.g. MMF_SkeletonAlpha) authored as
    // a one-way "dim it" effect with no meaningful reverse animation.
    // RestoreInitialValues(), not ResetFeedbacks(), is the MMFeedbacks call
    // that actually does this — ResetFeedbacks only rewinds play-count/
    // cooldown bookkeeping (CustomReset), it never touches a feedback's
    // target values (that's CustomRestoreInitialValues, reached only via
    // RestoreInitialValues).
    public void OnDone()
    {
        feedback?.RestoreInitialValues();
    }
}
