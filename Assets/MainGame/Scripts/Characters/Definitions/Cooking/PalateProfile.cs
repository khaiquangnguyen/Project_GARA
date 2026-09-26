using System;
using UnityEngine;

namespace GARA.Characters
{
    // Authored on an enemy's CharacterDefinition — whether it can be fed and
    // which flavors it favors (never Umami, which is dish-only).
    [Serializable]
    public struct PalateProfile
    {
        public const int MaxFullness = 100;

        [SerializeField] private bool canBeFed;
        [SerializeField] private FlavorTag favoriteFlavors;

        public bool CanBeFed => canBeFed;
        public FlavorTag FavoriteFlavors => favoriteFlavors.VisibleOnly();

        public static PalateProfile None => new PalateProfile { canBeFed = false };
    }
}
