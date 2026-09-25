using System;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Authored description of one entry in a RecipeSkillCard's steps array,
    // parallel by index to its InputSetCollectionDefinition's sets.
    [Serializable]
    public struct RecipeStep
    {
        public string stepName;
        public CookingStepKind kind;
        public Sprite icon;
    }
}
