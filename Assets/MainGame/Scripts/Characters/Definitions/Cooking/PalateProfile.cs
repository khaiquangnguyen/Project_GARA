using System;
using UnityEngine;

namespace GARA.Characters
{
    // Authored on an enemy's CharacterDefinition — whether it can be fed at
    // all, which visible flavors it favors, and how much fullness it takes
    // before a food coma is triggered (see CombatParticipant.Feed).
    [Serializable]
    public struct PalateProfile
    {
        [SerializeField] private bool canBeFed;
        [SerializeField] private FlavorTag favoriteFlavors;
        [SerializeField] private int fullnessCapacity;

        public bool CanBeFed => canBeFed && fullnessCapacity > 0;
        public FlavorTag FavoriteFlavors => favoriteFlavors.VisibleOnly();
        public int FullnessCapacity => fullnessCapacity;

        public int CountMatches(FlavorTag dishFlavors)
        {
            return dishFlavors.CountMatches(FavoriteFlavors);
        }

        public static PalateProfile None => new PalateProfile { canBeFed = false };

        internal void SanitizeInPlace(UnityEngine.Object context)
        {
            if ((favoriteFlavors & FlavorTag.Umami) != 0)
            {
                Debug.LogWarning($"{context?.name}: PalateProfile.favoriteFlavors included Umami, which is a hidden flavor and cannot be a favorite. Stripping it.", context);
                favoriteFlavors &= FlavorTags.Visible;
            }

            if (fullnessCapacity < 0)
            {
                fullnessCapacity = 0;
            }
        }
    }
}
