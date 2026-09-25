using System;
using GARA.Input;
using UnityEngine;

namespace GARA.Shake
{
    /// <summary>Two alternating tokens counted as completed pairs, with an optional time cap.</summary>
    [CreateAssetMenu(menuName = "GARA/Shake/Shake Input", fileName = "ShakeInput")]
    public class ShakeInputDefinition : ScriptableObject
    {
        [SerializeField]
        private InputToken first;

        [SerializeField]
        private InputToken second;

        /// <summary>Zero means run until Stop() is called externally.</summary>
        [SerializeField]
        private float duration;

        [SerializeField]
        private bool requireStartWithFirst;

        [SerializeField]
        private bool wrongPressBreaksStreak = true;

        public InputToken First => first;
        public InputToken Second => second;
        public float Duration => duration;
        public bool RequireStartWithFirst => requireStartWithFirst;
        public bool WrongPressBreaksStreak => wrongPressBreaksStreak;

        private void OnValidate()
        {
            if (first == second)
            {
                Debug.LogWarning($"[{name}] ShakeInputDefinition: first and second tokens are the same.", this);
            }
        }
    }
}
