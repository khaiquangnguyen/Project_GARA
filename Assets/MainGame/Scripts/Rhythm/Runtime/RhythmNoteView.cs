using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>
    /// Visual for a single traveling note: moves from a spawn point to a target point as its
    /// hit time approaches, carrying an approach circle that shrinks to rest exactly on the hit time.
    /// Once judged it only plays its hit or miss <see cref="MMF_Player"/>, which owns how the note
    /// looks as it resolves and ends by disabling it — returning it to the driver's pool. After
    /// judgement the note moves itself at approach speed, independent of the sequence ending: a hit
    /// note keeps going until it reaches the target and stops there (a late hit, already past it,
    /// stops immediately); a missed note keeps drifting. Pooled: <see cref="Place"/> undoes whatever the previous
    /// use's feedbacks changed.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class RhythmNoteView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        // Must be a child of this note (validated in OnValidate). Swap for Odin's
        // [ChildGameObjectsOnly] once Odin Inspector is in the project.
        [Tooltip("This note's approach circle — a child object. Its authored scale is the at-hit-time size; the driver scales it by the spec as the note approaches.")]
        [SerializeField]
        private Transform approachCircle;

        [Tooltip("Played once when this note is hit (any judgement but Miss). Must end by disabling the note, which returns it to the pool.")]
        [SerializeField]
        private MMF_Player hitFeedback;

        [Tooltip("Played once when this note is missed, while the note keeps drifting. Must end by disabling the note, which returns it to the pool.")]
        [SerializeField]
        private MMF_Player missFeedback;

        private Vector3 _spawnPosition;
        private Vector3 _targetPosition;
        private Vector3 _approachCircleBaseScale;
        private Vector3 _baseScale;
        private Color _baseColor;
        private Vector3 _driftVelocity;
        private bool _stopAtTarget;

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (approachCircle == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmNoteView)} on '{name}' has no approach circle assigned.");
            }

            // Pooled instances are created inactive, so this first runs on first use —
            // still the authored prefab values, which Place restores on every reuse.
            _approachCircleBaseScale = approachCircle.localScale;
            _baseScale = transform.localScale;
            _baseColor = spriteRenderer.color;

            if (hitFeedback == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmNoteView)} on '{name}' has no hit feedback assigned.");
            }

            if (missFeedback == null)
            {
                throw new InvalidOperationException($"{nameof(RhythmNoteView)} on '{name}' has no miss feedback assigned.");
            }
        }

        private void OnValidate()
        {
            if (approachCircle != null && (approachCircle == transform || !approachCircle.IsChildOf(transform)))
            {
                Debug.LogWarning($"{nameof(RhythmNoteView)} on '{name}': approach circle must be a child of this note — cleared.", this);
                approachCircle = null;
            }
        }

        private void Update()
        {
            if (_driftVelocity == Vector3.zero)
            {
                return;
            }

            transform.position += _driftVelocity * Time.deltaTime;

            if (_stopAtTarget && HasReachedTarget())
            {
                transform.position = _targetPosition;
                _driftVelocity = Vector3.zero;
            }
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        /// <summary>Call right after taking this note from the pool (and activating it). Resets everything a previous use's feedbacks may have changed.</summary>
        public void Place(Vector3 spawnPosition, Vector3 targetPosition, Quaternion rotation)
        {
            _spawnPosition = spawnPosition;
            _targetPosition = targetPosition;
            _driftVelocity = Vector3.zero;
            _stopAtTarget = false;
            transform.SetPositionAndRotation(spawnPosition, rotation);
            transform.localScale = _baseScale;
            spriteRenderer.color = _baseColor;
            approachCircle.localScale = _approachCircleBaseScale;
            approachCircle.gameObject.SetActive(true);
        }

        /// <summary>
        /// 0 = at spawn, 1 = at target. Values beyond 1 are allowed so a note can drift past the target before it's judged a miss.
        /// <paramref name="approachCircleScale"/> multiplies the approach circle's authored scale.
        /// </summary>
        public void SetProgress(float t, float approachCircleScale)
        {
            transform.position = Vector3.LerpUnclamped(_spawnPosition, _targetPosition, t);

            approachCircle.localScale = _approachCircleBaseScale * approachCircleScale;
        }

        /// <summary><paramref name="approachVelocity"/> is world units/sec; pass the note's approach velocity so an early hit finishes its travel seamlessly.</summary>
        public void PlayHit(Vector3 approachVelocity)
        {
            _stopAtTarget = true;
            _driftVelocity = HasReachedTarget() ? Vector3.zero : approachVelocity;
            hitFeedback.PlayFeedbacks();
        }

        /// <summary><paramref name="approachVelocity"/> is world units/sec; pass the note's approach velocity so the motion continues seamlessly.</summary>
        public void PlayMiss(Vector3 approachVelocity)
        {
            _stopAtTarget = false;
            _driftVelocity = approachVelocity;
            missFeedback.PlayFeedbacks();
        }

        private bool HasReachedTarget()
        {
            return Vector3.Dot(transform.position - _targetPosition, _targetPosition - _spawnPosition) >= 0f;
        }
    }
}
