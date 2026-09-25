using System;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// A bar showing how much of a time limit is left: shrinks its fill along local X and tints it.
    /// The fill's sprite pivot should sit on the edge the bar drains toward. Colors come from the
    /// driver's <see cref="InputSetVisualSpec"/>.
    /// </summary>
    public class InputSetTimerView : MonoBehaviour
    {
        [Tooltip("Child scaled along X from 0 (out of time) to its authored scale (full).")]
        [SerializeField]
        private Transform fill;

        [SerializeField]
        private SpriteRenderer fillRenderer;

        private Vector3 _fullScale;

        private void Awake()
        {
            if (fill == null)
            {
                throw new InvalidOperationException($"{nameof(InputSetTimerView)} on '{name}' has no fill assigned.");
            }

            if (fillRenderer == null)
            {
                fillRenderer = fill.GetComponent<SpriteRenderer>();
            }

            _fullScale = fill.localScale;
        }

        /// <summary><paramref name="normalizedRemaining"/>: 1 = full, 0 = out of time.</summary>
        public void SetFill(float normalizedRemaining, Color color)
        {
            var t = Mathf.Clamp01(normalizedRemaining);
            fill.localScale = new Vector3(_fullScale.x * t, _fullScale.y, _fullScale.z);

            if (fillRenderer != null)
            {
                fillRenderer.color = color;
            }
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
