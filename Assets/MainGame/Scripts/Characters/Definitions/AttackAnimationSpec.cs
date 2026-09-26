using Spine.Unity;
using UnityEngine;

namespace GARA.Characters
{
    // One attack clip plus the data combat needs to stage it (e.g. how far
    // from the target the attacker stands so the swing visually connects).
    [CreateAssetMenu(menuName = "GARA/Characters/Attack Animation Spec", fileName = "AttackAnimationSpec")]
    public class AttackAnimationSpec : ScriptableObject
    {
        [Tooltip("Only feeds the Animation dropdown below.")]
        [SerializeField]
        private SkeletonDataAsset skeletonData;

        [SpineAnimation(dataField: nameof(skeletonData))]
        [SerializeField]
        private string animationName;

        [Tooltip("X distance from the target the attacker stands at when it moves in to play this clip.")]
        [Min(0f)]
        [SerializeField]
        private float range = 1f;

        [Tooltip("Prefab (AttackHitFeedback at its root) spawned on the attacker and played on this move's hit in a live card (a bar or the finale).")]
        [SerializeField]
        private GameObject hitFeedback;

        [Tooltip("Impact (effects, hit feedback) on every \"hit\" event of the clip, not just the first — for multi-hit clips.")]
        [SerializeField]
        private bool impactOnEveryHit;

        public string AnimationName => animationName;
        public float Range => range;
        public GameObject HitFeedback => hitFeedback;
        public bool ImpactOnEveryHit => impactOnEveryHit;
    }
}
