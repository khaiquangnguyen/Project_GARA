using System;
using GARA.Characters;
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

        [Tooltip("Each play picks at random between the clip above and these. Empty = always the clip above.")]
        [SpineAnimation(dataField: nameof(skeletonRendererRef))]
        [SerializeField] private string[] alternateClips = Array.Empty<string>();

        [FormerlySerializedAs("_loop")]
        [SerializeField] private bool loop;

        protected override string ClipName => animationName;
        protected override bool Loop => loop;

        public override void Enter(CharacterStateContext context)
        {
            var pick = UnityEngine.Random.Range(0, alternateClips.Length + 1);
            if (pick > 0 && !string.IsNullOrEmpty(alternateClips[pick - 1]))
            {
                OverrideNextClip(alternateClips[pick - 1]);
            }

            base.Enter(context);
        }
    }
}
