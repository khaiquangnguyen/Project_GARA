using System;
using UnityEngine;

namespace GARA.Input
{
    /// <summary>
    /// A designer-assigned input identity, deliberately not an enum: nothing ties a token
    /// to a concrete button, a character class, or a specific input system.
    /// </summary>
    [Serializable]
    public struct InputToken : IEquatable<InputToken>
    {
        [SerializeField]
        private int id;

        public int Id => id;

        public InputToken(int id)
        {
            this.id = id;
        }

        public bool Equals(InputToken other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is InputToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return $"InputToken({id})";
        }

        public static bool operator ==(InputToken left, InputToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputToken left, InputToken right)
        {
            return !left.Equals(right);
        }
    }
}
