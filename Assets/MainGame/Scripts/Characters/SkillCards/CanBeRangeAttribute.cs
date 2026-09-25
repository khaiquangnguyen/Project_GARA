using System;
using UnityEngine;

namespace GARA.Characters
{
    // Marks a RangeableInt/RangeableFloat field as designer-authorable as a
    // range. Documentation + inspector affordance only — resolution behavior
    // lives entirely in the struct's Resolve method, no reflection anywhere.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CanBeRangeAttribute : PropertyAttribute
    {
    }
}
