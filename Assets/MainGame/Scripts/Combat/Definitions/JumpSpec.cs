using UnityEngine;

namespace GARA.Combat
{
    /// <summary>
    /// Jump tuning values, read by CombatSceneManager.Jump and JumpState.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Combat/Jump Spec", fileName = "JumpSpec")]
    public class JumpSpec : ScriptableObject
    {
        [Header("Timing")]
        [Tooltip("Seconds a dodge window stays open after the Jump input is pressed. A hit whose impact frame lands inside it is dodged.")]
        [SerializeField]
        private float windowDurationSeconds = 0.2f;

        [Tooltip("Seconds after a dodge window closes before Jump can be pressed again, whether or not it dodged anything.")]
        [SerializeField]
        private float cooldownSeconds = 0.3f;

        [Header("Jump Arc")]
        [Tooltip("Seconds the jump takes from takeoff to landing — the whole Jump Curve is played over this time.")]
        [SerializeField]
        private float jumpDurationSeconds = 0.5f;

        [Tooltip("World-unit height the character rises when Jump Curve reaches 1.")]
        [SerializeField]
        private float jumpHeight = 1.5f;

        [Tooltip("Height over the jump. X is normalized time (0 = takeoff, 1 = landing, stretched over Jump Duration Seconds); Y is multiplied by Jump Height.")]
        [SerializeField]
        private AnimationCurve jumpCurve = new(
            new Keyframe(0f, 0f, 0f, 4f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, -4f, 0f));

        public float WindowDurationSeconds => windowDurationSeconds;
        public float CooldownSeconds => cooldownSeconds;
        public float JumpDurationSeconds => jumpDurationSeconds;
        public float JumpHeight => jumpHeight;
        public AnimationCurve JumpCurve => jumpCurve;
    }
}
