using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// Visual for one token in the current set's row. Its persistent look (pending / current /
    /// accepted) is a tint from the driver's <see cref="InputSetVisualSpec"/>; everything transient
    /// is an <see cref="MMF_Player"/>. The optional state feedbacks can overlap each other and the
    /// tint, so they should animate scale/position/child objects rather than this sprite's color.
    /// Once its set finishes the token only plays its cleared or failed exit feedback, which owns
    /// how it leaves and ends by disabling it — returning it to the driver's pool. Pooled:
    /// <see cref="Place"/> undoes whatever the previous use's feedbacks changed.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class InputSetTokenView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [Tooltip("Optional. Played when this token becomes the next one to press, and stopped when it stops being it — e.g. a looping pulse.")]
        [SerializeField]
        private MMF_Player currentFeedback;

        [Tooltip("Optional. Played once when this token is pressed correctly.")]
        [SerializeField]
        private MMF_Player acceptedFeedback;

        [Tooltip("Optional. Played once when a wrong token is pressed while this one was expected. The retry starts on the same frame, so keep it a short flash/shake.")]
        [SerializeField]
        private MMF_Player wrongFeedback;

        [Tooltip("Optional. Played once on an accepted token when its set restarts for a retry.")]
        [SerializeField]
        private MMF_Player resetFeedback;

        [Tooltip("Played once when this token's set is cleared. Must end by disabling the token, which returns it to the pool.")]
        [SerializeField]
        private MMF_Player clearedExitFeedback;

        [Tooltip("Played once when this token's set fails for good (attempts exhausted, set collection timed out or aborted). Must end by disabling the token, which returns it to the pool.")]
        [SerializeField]
        private MMF_Player failedExitFeedback;

        private Sprite _baseSprite;
        private Vector3 _baseScale;
        private bool _isCurrent;

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

            // Pooled instances are created inactive, so this first runs on first use —
            // still the authored prefab values, which Place restores on every reuse.
            _baseSprite = spriteRenderer.sprite;
            _baseScale = transform.localScale;

            if (clearedExitFeedback == null)
            {
                throw new InvalidOperationException($"{nameof(InputSetTokenView)} on '{name}' has no cleared exit feedback assigned.");
            }

            if (failedExitFeedback == null)
            {
                throw new InvalidOperationException($"{nameof(InputSetTokenView)} on '{name}' has no failed exit feedback assigned.");
            }
        }

        /// <summary>Call right after taking this token from the pool (and activating it). <paramref name="sprite"/> null keeps the prefab's sprite.</summary>
        public void Place(Vector3 position, Sprite sprite, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = _baseScale;
            spriteRenderer.sprite = sprite != null ? sprite : _baseSprite;
            _isCurrent = false;
        }

        public void SetState(InputSetTokenState state, Color color)
        {
            spriteRenderer.color = color;

            var isCurrent = state == InputSetTokenState.Current;
            if (isCurrent == _isCurrent)
            {
                return;
            }

            _isCurrent = isCurrent;
            if (isCurrent)
            {
                Play(currentFeedback);
            }
            else
            {
                StopCurrent();
            }
        }

        public void PlayAccepted()
        {
            StopTransients();
            Play(acceptedFeedback);
        }

        public void PlayWrong()
        {
            Play(wrongFeedback);
        }

        public void PlayReset()
        {
            StopTransients();
            Play(resetFeedback);
        }

        public void PlayClearedExit()
        {
            StopCurrent();
            StopTransients();
            clearedExitFeedback.PlayFeedbacks();
        }

        public void PlayFailedExit()
        {
            StopCurrent();
            StopTransients();
            failedExitFeedback.PlayFeedbacks();
        }

        // Tracked here rather than read from the player: a repeat-forever feedback doesn't keep
        // MMF_Player.IsPlaying reliably true.
        private void StopCurrent()
        {
            _isCurrent = false;
            if (currentFeedback != null)
            {
                currentFeedback.StopFeedbacks();
            }
        }

        private void StopTransients()
        {
            Stop(acceptedFeedback);
            Stop(wrongFeedback);
            Stop(resetFeedback);
        }

        private static void Play(MMF_Player feedback)
        {
            if (feedback != null)
            {
                feedback.PlayFeedbacks();
            }
        }

        private static void Stop(MMF_Player feedback)
        {
            if (feedback != null && feedback.IsPlaying)
            {
                feedback.StopFeedbacks();
            }
        }
    }
}
