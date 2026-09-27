using UnityEngine;

namespace GARA.Combat
{
    // Raised when a hit is negated by invulnerability (see
    // CombatParticipant.ApplyDamage).
    public readonly struct InvulnerableBlockStateEvent
    {
        public readonly GameObject sceneRoot;

        public InvulnerableBlockStateEvent(GameObject sceneRoot)
        {
            this.sceneRoot = sceneRoot;
        }
    }
}
