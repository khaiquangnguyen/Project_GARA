using UnityEngine;

namespace GARA.Combat
{
    // Broadcast via MoreMountains.Tools' MMEventManager whenever a
    // participant's targeting state changes for the action about to play —
    // isTargeted is true for whoever's actually selected, false for every
    // other living candidate in that pool. This event says nothing about
    // what should happen as a result (dim, particle, sound, nothing at
    // all) — that's entirely up to whatever's listening (see
    // OnNotTargetedEffect). Carries only a GameObject (the participant's
    // SceneRoot), not a CombatParticipant, so listeners in the plain
    // (non-GARA.Combat) assembly don't need to know about GARA.Combat's
    // runtime types beyond "which GameObject is this about."
    public readonly struct TargetedStateEvent
    {
        public readonly GameObject sceneRoot;
        public readonly bool isTargeted;

        public TargetedStateEvent(GameObject sceneRoot, bool isTargeted)
        {
            this.sceneRoot = sceneRoot;
            this.isTargeted = isTargeted;
        }
    }
}
