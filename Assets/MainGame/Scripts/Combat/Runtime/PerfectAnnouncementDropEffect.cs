using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace GARA.Combat
{
    // Root of a card's perfect-announcement drop: slides or slams onto the
    // target, landing exactly when Land() is called (the finale's hit frame),
    // then holds, exits and destroys itself.
    public class PerfectAnnouncementDropEffect : MonoBehaviour
    {
        // Share of the fall played before Land() — keeps it from arriving
        // a frame ahead of the hit.
        private const float MaxProgressBeforeLand = 0.99f;

        // Seconds past duration it waits for Land() before landing anyway.
        private const float LandTimeout = 0.5f;

        // Overshoot of the settle's ease-out-back — low, so it snaps rather
        // than wobbles.
        private const float SettleOvershoot = 1.2f;

        // Share of the slam's time it takes to turn fully opaque — early, so
        // it reads as solid before it hits instead of fading in on impact.
        private const float SlamOpaqueAt = 0.4f;

        // Several drops land on one hit; only the first freezes time.
        private static int _lastHitstopFrame = -1;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private AnnouncementMotion motion = AnnouncementMotion.Slide;

        [SerializeField]
        private MMTweenType ease = new MMTweenType(MMTween.MMTweenCurve.EaseInQuadratic);

        [Tooltip("Seconds from release to landing.")]
        [Min(0f)]
        [SerializeField]
        private float duration = 0.4f;

        [Tooltip("Landing point, relative to the target's body centre.")]
        [SerializeField]
        private Vector2 landingOffset;

        [Header("Slide")]
        [Tooltip("The side it launches from, relative to where it lands.")]
        [SerializeField]
        private DropLaunchDirection direction = DropLaunchDirection.Up;

        [Tooltip("World units from the landing point it launches from.")]
        [Min(0f)]
        [SerializeField]
        private float distance = 3f;

        [Header("Slam")]
        [Tooltip("Scale it starts the slam at; lands at 1.")]
        [Min(0f)]
        [SerializeField]
        private float slamStartScale = 3f;

        [Tooltip("Alpha it starts the slam at; lands fully opaque.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float slamStartAlpha = 0.25f;

        [Tooltip("Degrees of tilt it lands with, settling to upright.")]
        [SerializeField]
        private float landTilt = 8f;

        [Tooltip("Scale on the landing frame, springing back to 1 — wide and short reads as a squash.")]
        [SerializeField]
        private Vector2 landSquash = new Vector2(1.2f, 0.75f);

        [Tooltip("Seconds the squash and tilt take to settle.")]
        [Min(0f)]
        [SerializeField]
        private float settleDuration = 0.18f;

        [Header("Impact")]
        [Tooltip("Seconds of freeze frame on landing (needs an MMTimeManager in the scene). 0 for none.")]
        [Min(0f)]
        [SerializeField]
        private float hitstopDuration;

        [Tooltip("World units it jitters on landing, decaying over shakeDuration. 0 for none.")]
        [Min(0f)]
        [SerializeField]
        private float shakeStrength;

        [Min(0f)]
        [SerializeField]
        private float shakeDuration = 0.15f;

        [Tooltip("Optional. Played on landing — e.g. camera shake or a flash.")]
        [SerializeField]
        private MMF_Player landFeedback;

        [Tooltip("Optional. Played on landing — copies of the stamp bursting out from it. Keep it off the sprite so the squash and shake don't carry it.")]
        [SerializeField]
        private ParticleSystem echoParticles;

        [Tooltip("Optional. Prefab spawned where it lands, on landing — e.g. confetti. A particle system at its root with a Sprite Renderer shape bursts out of this stamp's text.")]
        [SerializeField]
        private GameObject landBurst;

        [Header("Hold & Exit")]
        [Tooltip("Seconds it stays on the target after landing before it exits.")]
        [Min(0f)]
        [SerializeField]
        private float holdDuration = 0.5f;

        [Tooltip("Seconds it takes to grow and fade out after the hold. 0 to vanish.")]
        [Min(0f)]
        [SerializeField]
        private float exitDuration;

        [Tooltip("Scale it grows to while exiting.")]
        [Min(0f)]
        [SerializeField]
        private float exitScale = 1.3f;

        private Transform _spriteTransform;
        private Vector3 _spriteBaseScale;
        private Vector3 _spriteBasePosition;
        private Color _spriteBaseColor;
        private Vector3 _startPosition;
        private Vector3 _landingPosition;
        private float _delay;
        private float _elapsed;
        private float _sinceLanded;
        private bool _isDropping;
        private bool _hasLanded;

        public float Duration => duration;

        private bool EchoesAlive => echoParticles != null && echoParticles.IsAlive(true);

        // Releases the drop after delay seconds, landing on the target
        // duration seconds later.
        public void Drop(Vector3 targetCentre, float delay)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                _spriteTransform = spriteRenderer.transform;
                _spriteBaseScale = _spriteTransform.localScale;
                _spriteBasePosition = _spriteTransform.localPosition;
                _spriteBaseColor = spriteRenderer.color;
            }

            _landingPosition = targetCentre + (Vector3)landingOffset;
            _startPosition = motion == AnnouncementMotion.Slam ? _landingPosition : _landingPosition + LaunchOffset();
            _delay = delay;
            _elapsed = 0f;
            _sinceLanded = 0f;
            _isDropping = true;
            _hasLanded = false;
            transform.position = _startPosition;
            ApplyFall(0f, 0f);
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
            ApplyLanded(0f);

            if (echoParticles != null)
            {
                echoParticles.Play(true);
            }

            if (landBurst != null)
            {
                var burst = Instantiate(landBurst, _landingPosition, Quaternion.identity);
                if (spriteRenderer != null && burst.TryGetComponent<ParticleSystem>(out var particles))
                {
                    var shape = particles.shape;
                    if (shape.shapeType == ParticleSystemShapeType.SpriteRenderer)
                    {
                        shape.spriteRenderer = spriteRenderer;
                    }
                }
            }

            if (hitstopDuration > 0f && _lastHitstopFrame != Time.frameCount)
            {
                _lastHitstopFrame = Time.frameCount;
                MMFreezeFrameEvent.Trigger(hitstopDuration, 0f);
            }

            if (landFeedback != null)
            {
                landFeedback.PlayFeedbacks();
            }
        }

        private void Update()
        {
            if (!_isDropping)
            {
                return;
            }

            if (_hasLanded)
            {
                _sinceLanded += Time.deltaTime;
                if (_sinceLanded >= holdDuration + exitDuration && !EchoesAlive)
                {
                    Destroy(gameObject);
                    return;
                }

                ApplyLanded(_sinceLanded);
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
            ApplyFall(ease.Evaluate(progress), progress);
        }

        // t is the eased progress, progress the raw one.
        private void ApplyFall(float t, float progress)
        {
            if (motion == AnnouncementMotion.Slide)
            {
                transform.position = Vector3.LerpUnclamped(_startPosition, _landingPosition, t);
                return;
            }

            SetSpriteTransform(Vector3.one * Mathf.LerpUnclamped(slamStartScale, 1f, t), 0f, Vector3.zero);
            SetAlpha(Mathf.Lerp(slamStartAlpha, 1f, progress / SlamOpaqueAt));
        }

        // Squash, tilt and shake settling after the hit, then the exit.
        private void ApplyLanded(float since)
        {
            var scale = Vector3.one;
            var tilt = 0f;
            if (motion == AnnouncementMotion.Slam && settleDuration > 0f && since < settleDuration)
            {
                var settle = EaseOutBack(since / settleDuration);
                scale = Vector3.LerpUnclamped(new Vector3(landSquash.x, landSquash.y, 1f), Vector3.one, settle);
                tilt = Mathf.LerpUnclamped(landTilt, 0f, settle);
            }

            var shake = Vector3.zero;
            if (shakeStrength > 0f && shakeDuration > 0f && since < shakeDuration)
            {
                shake = (Vector3)(Random.insideUnitCircle * (shakeStrength * (1f - since / shakeDuration)));
            }

            var alpha = 1f;
            var exiting = since - holdDuration;
            if (exitDuration > 0f && exiting > 0f)
            {
                var exit = Mathf.Clamp01(exiting / exitDuration);
                scale *= Mathf.Lerp(1f, exitScale, exit);
                alpha = 1f - exit;
            }
            else if (exiting >= 0f && exitDuration <= 0f)
            {
                alpha = 0f;
            }

            SetSpriteTransform(scale, tilt, shake);
            SetAlpha(alpha);
        }

        private void SetSpriteTransform(Vector3 scale, float tilt, Vector3 offset)
        {
            if (_spriteTransform == null)
            {
                return;
            }

            _spriteTransform.localScale = Vector3.Scale(_spriteBaseScale, scale);
            _spriteTransform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            _spriteTransform.localPosition = _spriteBasePosition + offset;
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var color = _spriteBaseColor;
            color.a *= alpha;
            spriteRenderer.color = color;
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

        private static float EaseOutBack(float t)
        {
            var u = t - 1f;
            return 1f + (SettleOvershoot + 1f) * u * u * u + SettleOvershoot * u * u;
        }
    }
}
