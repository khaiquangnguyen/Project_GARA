using UnityEngine;

namespace GARA.Combat
{
    // Raised when a hit is dodged by a jump (see
    // CombatParticipant.ApplyDamage).
    public readonly struct JumpSuccessStateEvent
    {
        public readonly GameObject sceneRoot;

        public JumpSuccessStateEvent(GameObject sceneRoot)
        {
            this.sceneRoot = sceneRoot;
        }
    }
}
