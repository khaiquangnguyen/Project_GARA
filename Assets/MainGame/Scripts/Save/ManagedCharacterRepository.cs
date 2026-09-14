using System.Collections.Generic;
using GARA.Characters;

namespace GARA.Save
{
    // Thin Easy Save 3 adapter for the roster. Kept out of GARA.Characters
    // (which has no asmdef of its own to reference) so ManagedCharacter stays
    // a plain POCO with no dependency on the save plugin.
    public static class ManagedCharacterRepository
    {
        private const string RosterKeyPrefix = "roster_";
        private const string RosterIndexKey = "roster_index";

        public static void Save(ManagedCharacter character)
        {
            ES3.Save(RosterKeyPrefix + character.instanceId, character);

            var index = LoadIndex();
            if (!index.Contains(character.instanceId))
            {
                index.Add(character.instanceId);
                ES3.Save(RosterIndexKey, index);
            }
        }

        public static bool TryLoad(string instanceId, out ManagedCharacter character)
        {
            var key = RosterKeyPrefix + instanceId;
            if (ES3.KeyExists(key))
            {
                character = ES3.Load<ManagedCharacter>(key);
                return true;
            }

            character = null;
            return false;
        }

        public static List<ManagedCharacter> LoadRoster()
        {
            var roster = new List<ManagedCharacter>();
            foreach (var instanceId in LoadIndex())
            {
                if (TryLoad(instanceId, out var character))
                {
                    roster.Add(character);
                }
            }

            return roster;
        }

        private static List<string> LoadIndex()
        {
            return ES3.KeyExists(RosterIndexKey)
                ? ES3.Load<List<string>>(RosterIndexKey)
                : new List<string>();
        }
    }
}
