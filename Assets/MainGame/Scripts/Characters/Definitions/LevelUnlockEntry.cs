using System;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Abilities themselves aren't designed yet, so this only carries an ID
    // placeholder rather than a reference to an ability definition type.
    [Serializable]
    public struct LevelUnlockEntry
    {

        public int level;

        public string abilityId;
    }
}
