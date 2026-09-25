using System.Collections;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>Visual for the fixed judgment point: pulses when a note's timing window opens, and flashes to report a judgement. Colors and pulse come from the driver's <see cref="RhythmVisualSpec"/>.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class RhythmTargetView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private RhythmVisualSpec _spec;
        private Color _restColor;
        private Vector3 _restScale;
        private Coroutine _pulseRoutine;

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

            _restColor = spriteRenderer.color;
            _restScale = transform.localScale;
        }

        public void Configure(RhythmVisualSpec spec)
        {
            _spec = spec;
        }

        /// <summary>Call when a note's timing window opens, cueing the player that it's time to press.</summary>
        public void PlayReady()
        {
            PlayPulse(_spec.TargetReadyColor);
        }

        public void PlayJudgement(RhythmJudgement judgement)
        {
            PlayPulse(_spec.ColorFor(judgement));
        }

        private void PlayPulse(Color flashColor)
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
            }

            _pulseRoutine = StartCoroutine(PulseRoutine(flashColor));
        }

        private IEnumerator PulseRoutine(Color flashColor)
        {
            var peakScale = _restScale * _spec.TargetPulseScale;
            var duration = _spec.TargetPulseDuration;
            transform.localScale = peakScale;
            spriteRenderer.color = flashColor;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.localScale = Vector3.Lerp(peakScale, _restScale, t);
                spriteRenderer.color = Color.Lerp(flashColor, _restColor, t);
                yield return null;
            }

            transform.localScale = _restScale;
            spriteRenderer.color = _restColor;
            _pulseRoutine = null;
        }
    }
}
