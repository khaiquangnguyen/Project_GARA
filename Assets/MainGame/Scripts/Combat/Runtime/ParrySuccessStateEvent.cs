using UnityEngine;

namespace GARA.Combat
{
    // Raised when a hit is parried (see CombatParticipant.ApplyDamage).
    public readonly struct ParrySuccessStateEvent
    {
        public readonly GameObject sceneRoot;

        public ParrySuccessStateEvent(GameObject sceneRoot)
        {
            this.sceneRoot = sceneRoot;
        }
    }
}
