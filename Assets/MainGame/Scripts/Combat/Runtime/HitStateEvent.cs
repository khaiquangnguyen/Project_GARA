using UnityEngine;

namespace GARA.Combat
{
    // Broadcast via MoreMountains.Tools' MMEventManager whenever a
    // participant actually takes damage (see CombatParticipant.ApplyDamage).
    // Says nothing about what should happen as a result — that's entirely
    // up to whatever's listening (see OnHitEffect). Carries only a
    // GameObject (the participant's SceneRoot), not a CombatParticipant, so
    // listeners in the plain (non-GARA.Combat) assembly don't need to know
    // about GARA.Combat's runtime types beyond "which GameObject is this
    // about" — mirrors TargetedStateEvent.
    public readonly struct HitStateEvent
    {
        public readonly GameObject sceneRoot;

        public HitStateEvent(GameObject sceneRoot)
        {
            this.sceneRoot = sceneRoot;
        }
    }
}
