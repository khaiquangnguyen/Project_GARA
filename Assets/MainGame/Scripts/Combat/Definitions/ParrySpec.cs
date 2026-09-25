using UnityEngine;

namespace GARA.Combat
{
    /// <summary>
    /// Parry tuning values, read by CombatSceneManager.Parry.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Combat/Parry Spec", fileName = "ParrySpec")]
    public class ParrySpec : ScriptableObject
    {
        [Header("Timing")]
        [Tooltip("Seconds a parry window stays open after the Parry input is pressed. A hit whose impact frame lands inside it is parried.")]
        [SerializeField]
        private float windowDurationSeconds = 0.2f;

        [Tooltip("Seconds after a parry window closes without parrying anything before Parry can be pressed again. Skipped after a successful parry.")]
        [SerializeField]
        private float missCooldownSeconds = 0.3f;

        public float WindowDurationSeconds => windowDurationSeconds;
        public float MissCooldownSeconds => missCooldownSeconds;
    }
}
