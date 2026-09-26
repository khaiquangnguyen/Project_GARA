using UnityEngine;

namespace GARA.Combat
{
    // Broadcast via MMEventManager when a participant's turn starts
    // (isActive) and when it stops acting or its turn ends (see
    // OnActiveActorEffect). Mirrors TargetedStateEvent.
    public readonly struct ActiveActorStateEvent
    {
        public readonly GameObject sceneRoot;
        public readonly bool isActive;
        public readonly bool isPlayerControlled;

        public ActiveActorStateEvent(GameObject sceneRoot, bool isActive, bool isPlayerControlled)
        {
            this.sceneRoot = sceneRoot;
            this.isActive = isActive;
            this.isPlayerControlled = isPlayerControlled;
        }
    }
}