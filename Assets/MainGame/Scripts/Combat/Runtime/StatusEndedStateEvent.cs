using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Broadcast when a participant's last status of a kind wears off, so
    // per-character visuals (e.g. OnExorcisedEffect) can clear.
    public readonly struct StatusEndedStateEvent
    {
        public readonly GameObject sceneRoot;
        public readonly StatusEffectKind kind;

        public StatusEndedStateEvent(GameObject sceneRoot, StatusEffectKind kind)
        {
            this.sceneRoot = sceneRoot;
            this.kind = kind;
        }
    }
}
