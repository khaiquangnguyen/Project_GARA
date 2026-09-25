using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Combat
{
    // A non-attack state (idle, hit, death, ...) that plays a clip picked by
    // name. Attack states use an AttackAnimationSpec instead.
    public abstract class NamedSpineAnimationState : SpineAnimationState
    {
        // Feeds the [SpineAnimation] dropdown below — Spine's drawer can't
        // find a SkeletonRenderer on a parent by itself.
        [SerializeField] private SkeletonRenderer skeletonRendererRef;

        [FormerlySerializedAs("_animationName")]
        [SpineAnimation(dataField: nameof(skeletonRendererRef))]
        [SerializeField] private string animationName;
        [FormerlySerializedAs("_loop")]
        [SerializeField] private bool loop;

        protected override string ClipName => animationName;
        protected override bool Loop => loop;
    }
}
