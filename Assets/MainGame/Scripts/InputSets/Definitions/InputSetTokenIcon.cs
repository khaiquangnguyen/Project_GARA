using System;
using GARA.Input;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>How a <see cref="token"/> is drawn in an input set row.</summary>
    [Serializable]
    public struct InputSetTokenIcon
    {
        public InputToken token;
        public Sprite sprite;

        /// <summary>Z rotation in degrees (Unity 2D: counter-clockwise positive) — lets one arrow sprite serve every direction.</summary>
        public float rotationDegrees;
    }
}
