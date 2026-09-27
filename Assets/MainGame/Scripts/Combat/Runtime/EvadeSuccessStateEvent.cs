using UnityEngine;

namespace GARA.Combat
{
    // Raised when a hit is dodged by an evasion stack (see
    // CombatParticipant.ApplyDamage).
    public readonly struct EvadeSuccessStateEvent
    {
        public readonly GameObject sceneRoot;

        public EvadeSuccessStateEvent(GameObject sceneRoot)
        {
            this.sceneRoot = sceneRoot;
        }
    }
}
