using System;
using UnityEngine;

namespace GARA.Combat
{
    // One spot a character steps back to while an action it isn't targeted
    // by plays out (see CombatSceneManager.Retreat). Carries its own sorting
    // order, applied for as long as a character stands there, so each scene
    // decides how retreated characters layer against each other and
    // against whoever's still in formation (the actor renders at 1,
    // everyone else at 0 — keep these below 0 to put retreated characters
    // behind).
    [Serializable]
    public struct RetreatSlot
    {
        public Transform anchor;
        public int sortingOrder;
    }
}
