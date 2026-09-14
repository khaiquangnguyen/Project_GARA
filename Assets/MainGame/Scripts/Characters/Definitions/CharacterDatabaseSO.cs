using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Manually-assigned roster of character prefabs (each carrying a
    // CharacterDefinition component). No lookup/caching machinery — this
    // list is small and hand-curated, so callers scan it directly.
    [CreateAssetMenu(menuName = "GARA/Character/Character Database", fileName = "CharacterDatabase")]
    public class CharacterDatabaseSo : ScriptableObject
    {

        public List<CharacterDefinition> characters = new();
    }
}
