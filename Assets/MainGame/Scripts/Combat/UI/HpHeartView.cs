using MoreMountains.Feedbacks;
using UnityEngine;

namespace GARA.Combat
{
    // Heart HP gauge: the fill sprite sits inside a heart-shaped SpriteMask and
    // slides down as HP falls, so its visible height tracks the HP fraction.
    public class HpHeartView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer fill;
        [SerializeField] private SpriteMask mask;

        [Tooltip("Played whenever HP drops (the f1-f3 splash).")]
        [SerializeField] private MMF_Player hpLostFeedback;

        [SerializeField, Range(0f, 1f)]
        private float fraction = 1f;

        private void OnValidate()
        {
            ApplyFraction();
        }

        private void Awake()
        {
            ApplyFraction();
        }

        public void SetHp(int current, int max)
        {
            SetFraction(max > 0 ? (float)current / max : 0f);
        }

        public void SetFraction(float value)
        {
            var previous = fraction;
            fraction = Mathf.Clamp01(value);
            ApplyFraction();

            if (fraction < previous && hpLostFeedback != null && isActiveAndEnabled)
            {
                hpLostFeedback.PlayFeedbacks();
            }
        }

        // Full at local y = 0; empty once the fill's top drops below the
        // mask's bottom edge.
        private void ApplyFraction()
        {
            if (fill == null || fill.sprite == null)
            {
                return;
            }

            var height = mask != null && mask.sprite != null
                ? mask.sprite.bounds.size.y * mask.transform.localScale.y
                : fill.sprite.bounds.size.y * fill.transform.localScale.y;

            var position = fill.transform.localPosition;
            position.y = -(1f - fraction) * height;
            fill.transform.localPosition = position;
        }
    }
}
