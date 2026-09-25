using MoreMountains.Tools;
using UnityEngine;

namespace GARA.Combat
{
    // Root of a card's perfect-announcement drop: flies in from direction,
    // distance away, onto the target. Lands exactly when Land() is called
    // (the finale's hit frame), holds, then destroys itself.
    public class PerfectAnnouncementDropEffect : MonoBehaviour
    {
        // Share of the fall played before Land() — keeps it from arriving
        // a frame ahead of the hit.
        private const float MaxProgressBeforeLand = 0.99f;

        // Seconds past duration it waits for Land() before landing anyway.
        private const float LandTimeout = 0.5f;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [Tooltip("The side it launches from, relative to where it lands.")]
        [SerializeField]
        private DropLaunchDirection direction = DropLaunchDirection.Up;

        [Tooltip("World units from the landing point it launches from.")]
        [Min(0f)]
        [SerializeField]
        private float distance = 3f;

        [SerializeField]
        private MMTweenType ease = new MMTweenType(MMTween.MMTweenCurve.EaseInQuadratic);

        [Tooltip("Seconds from release to landing.")]
        [Min(0f)]
        [SerializeField]
        private float duration = 0.4f;

        [Tooltip("Seconds it stays on the target after landing before it's destroyed.")]
        [Min(0f)]
        [SerializeField]
        private float holdDuration = 0.5f;

        [Tooltip("Landing point, relative to the target's body centre.")]
        [SerializeField]
        private Vector2 landingOffset;

        private Vector3 _startPosition;
        private Vector3 _landingPosition;
        private float _delay;
        private float _elapsed;
        private float _heldFor;
        private bool _isDropping;
        private bool _hasLanded;

        public float Duration => duration;

        // Releases the drop after delay seconds, landing on the target
        // duration seconds later.
        public void Drop(Vector3 targetCentre, float delay)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _landingPosition = targetCentre + (Vector3)landingOffset;
            _startPosition = _landingPosition + LaunchOffset();
            _delay = delay;
            _elapsed = 0f;
            _heldFor = 0f;
            _isDropping = true;
            _hasLanded = false;
            transform.position = _startPosition;
            SetVisible(delay <= 0f);
        }

        public void Land()
        {
            if (!_isDropping || _hasLanded)
            {
                return;
            }

            _hasLanded = true;
            transform.position = _landingPosition;
            SetVisible(true);
        }

        private void Update()
        {
            if (!_isDropping)
            {
                return;
            }

            if (_hasLanded)
            {
                _heldFor += Time.deltaTime;
                if (_heldFor >= holdDuration)
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
                if (_delay > 0f)
                {
                    return;
                }

                SetVisible(true);
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= duration + LandTimeout)
            {
                Land();
                return;
            }

            var progress = duration > 0f ? Mathf.Min(_elapsed / duration, MaxProgressBeforeLand) : MaxProgressBeforeLand;
            transform.position = Vector3.LerpUnclamped(_startPosition, _landingPosition, ease.Evaluate(progress));
        }

        private Vector3 LaunchOffset()
        {
            switch (direction)
            {
                case DropLaunchDirection.Down:
                    return Vector3.down * distance;
                case DropLaunchDirection.Left:
                    return Vector3.left * distance;
                case DropLaunchDirection.Right:
                    return Vector3.right * distance;
                default:
                    return Vector3.up * distance;
            }
        }

        private void SetVisible(bool visible)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }
}
