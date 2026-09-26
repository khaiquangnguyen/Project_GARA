using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Broadcast via MMEventManager when a status lands on a participant (see
    // CombatParticipant.ApplyStatus), for per-character visuals such as
    // OnNoirifiedEffect. Mirrors HitStateEvent.
    public readonly struct StatusAppliedStateEvent
    {
        public readonly GameObject sceneRoot;
        public readonly StatusEffectKind kind;

        public StatusAppliedStateEvent(GameObject sceneRoot, StatusEffectKind kind)
        {
            this.sceneRoot = sceneRoot;
            this.kind = kind;
        }
    }
}
