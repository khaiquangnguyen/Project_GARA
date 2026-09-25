using System;
using GARA.Input;
using MoreMountains.Tools;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>
    /// Every presentation-only value of the rhythm visual system, kept out of the prefabs so
    /// tweaks made during a Play-mode test persist. Nothing here affects timing or judging —
    /// that stays in <see cref="RhythmSequenceDefinition"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Rhythm/Rhythm Visual Spec", fileName = "RhythmVisualSpec")]
    public class RhythmVisualSpec : ScriptableObject
    {
        [Header("Notes")]
        [Tooltip("Seconds a note spends traveling from the spawn point to the target point before its hit time. Lower = faster notes.")]
        [SerializeField]
        private float travelDuration = 1f;

        [Header("Note Direction")]
        [Tooltip("Which way the note prefab's arrow sprite points with no rotation applied. Notes are rotated from this to their input's direction.")]
        [SerializeField]
        private RhythmArrowDirection noteSpriteFacing = RhythmArrowDirection.Up;

        [Tooltip("Which way each input token's notes point. A token missing here keeps the sprite's default facing (with a warning).")]
        [SerializeField]
        private RhythmTokenDirection[] tokenDirections = Array.Empty<RhythmTokenDirection>();

        [Header("Approach Circle")]
        [Tooltip("Approach circle size (× its prefab scale) when its note spawns. It shrinks to End Scale exactly on the note's hit time.")]
        [SerializeField]
        private float approachCircleStartScale = 3f;

        [Tooltip("Approach circle size (× its prefab scale) on the note's hit time.")]
        [SerializeField]
        private float approachCircleEndScale = 1f;

        [Tooltip("Easing of the shrink from Start Scale to End Scale. Whatever the curve, the circle still lands on End Scale exactly at the hit time.")]
        [SerializeField]
        private MMTweenType approachCircleEase = new MMTweenType(MMTween.MMTweenCurve.LinearTween);

        [Header("Target")]
        [SerializeField]
        private Color targetReadyColor = Color.white;

        [SerializeField]
        private Color perfectColor = new Color(1f, 0.9215686f, 0.015686275f, 1f);

        [SerializeField]
        private Color goodColor = Color.green;

        [SerializeField]
        private Color okColor = Color.cyan;

        [SerializeField]
        private Color missColor = Color.red;

        [SerializeField]
        private float targetPulseScale = 1.2f;

        [SerializeField]
        private float targetPulseDuration = 0.12f;

        public float TravelDuration => travelDuration;
        public Color TargetReadyColor => targetReadyColor;
        public float TargetPulseScale => targetPulseScale;
        public float TargetPulseDuration => targetPulseDuration;

        /// <summary>Approach circle scale at note travel progress <paramref name="t"/> (0 = spawned, 1 = hit time); holds at End Scale past the hit time.</summary>
        public float ApproachCircleScaleAt(float t)
        {
            // Unclamped so overshooting eases (Back, Elastic) can dip past End Scale mid-shrink.
            var eased = approachCircleEase.Evaluate(Mathf.Clamp01(t));
            return Mathf.LerpUnclamped(approachCircleStartScale, approachCircleEndScale, eased);
        }

        /// <summary>Rotation to apply to a note for <paramref name="token"/> so its arrow faces that token's direction. False (and identity) if the token has no direction set.</summary>
        public bool TryGetNoteRotation(InputToken token, out Quaternion rotation)
        {
            foreach (var entry in tokenDirections)
            {
                if (entry.token == token)
                {
                    rotation = Quaternion.Euler(0f, 0f, entry.direction.ToDegrees() - noteSpriteFacing.ToDegrees());
                    return true;
                }
            }

            rotation = Quaternion.identity;
            return false;
        }

        public Color ColorFor(RhythmJudgement judgement)
        {
            switch (judgement)
            {
                case RhythmJudgement.Perfect:
                    return perfectColor;
                case RhythmJudgement.Good:
                    return goodColor;
                case RhythmJudgement.Ok:
                    return okColor;
                default:
                    return missColor;
            }
        }
    }
}
