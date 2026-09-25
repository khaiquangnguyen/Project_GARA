using System;
using GARA.Input;
using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// Every presentation-only value of the input set visual system, kept out of the prefabs so
    /// tweaks made during a Play-mode test persist. Nothing here affects judging — that stays in
    /// <see cref="InputSetCollectionDefinition"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Input Sets/Input Set Visual Spec", fileName = "InputSetVisualSpec")]
    public class InputSetVisualSpec : ScriptableObject
    {
        [Header("Layout")]
        [Tooltip("World-unit distance between neighbouring tokens. A set's tokens are spread evenly around the row anchor, along its right axis.")]
        [SerializeField]
        private float tokenSpacing = 1.25f;

        [Header("Tokens")]
        [Tooltip("Sprite and rotation per input token. A token missing here keeps the token prefab's sprite (with a warning).")]
        [SerializeField]
        private InputSetTokenIcon[] tokenIcons = Array.Empty<InputSetTokenIcon>();

        [SerializeField]
        private Color pendingColor = new Color(1f, 1f, 1f, 0.5f);

        [SerializeField]
        private Color currentColor = Color.white;

        [SerializeField]
        private Color acceptedColor = Color.green;

        [Header("Timers")]
        [SerializeField]
        private Color timerNormalColor = Color.white;

        [SerializeField]
        private Color timerWarningColor = Color.red;

        [Tooltip("Fraction of a timer left (0-1) at or below which it switches to the warning color.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float timerWarningThreshold = 0.25f;

        public float TokenSpacing => tokenSpacing;

        /// <summary>Sprite and rotation for <paramref name="token"/>. False (null sprite, identity) if the token has no icon set.</summary>
        public bool TryGetIcon(InputToken token, out Sprite sprite, out Quaternion rotation)
        {
            foreach (var entry in tokenIcons)
            {
                if (entry.token == token)
                {
                    sprite = entry.sprite;
                    rotation = Quaternion.Euler(0f, 0f, entry.rotationDegrees);
                    return true;
                }
            }

            sprite = null;
            rotation = Quaternion.identity;
            return false;
        }

        public Color ColorFor(InputSetTokenState state)
        {
            switch (state)
            {
                case InputSetTokenState.Current:
                    return currentColor;
                case InputSetTokenState.Accepted:
                    return acceptedColor;
                default:
                    return pendingColor;
            }
        }

        /// <summary>Timer color with <paramref name="normalizedRemaining"/> (1 = full, 0 = out) of its time left.</summary>
        public Color TimerColorAt(float normalizedRemaining)
        {
            return normalizedRemaining <= timerWarningThreshold ? timerWarningColor : timerNormalColor;
        }
    }
}
