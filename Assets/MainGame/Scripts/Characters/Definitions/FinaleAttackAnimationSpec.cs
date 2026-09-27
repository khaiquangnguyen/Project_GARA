using UnityEngine;

namespace GARA.Characters
{
    // A live card's perfect-finale clip: an attack spec plus optional
    // finale-only presentation, each its own field and played on its own —
    // e.g. a light effect over the stage, an announcement dropped onto each
    // target to land on its hit frame.
    [CreateAssetMenu(menuName = "GARA/Characters/Finale Attack Animation Spec", fileName = "FinaleAttackAnimationSpec")]
    public class FinaleAttackAnimationSpec : AttackAnimationSpec
    {
        [Tooltip("Optional. Prefab (AttackHitFeedback at its root) spawned on the attacker and played on the finale's hit, alongside the hit feedback — e.g. disco lights.")]
        [SerializeField]
        private GameObject lightEffect;

        [Tooltip("Optional. Prefab (PerfectAnnouncementDropEffect at its root) dropped onto each target on the perfect finale. Leave empty for a finale without a drop.")]
        [SerializeField]
        private GameObject perfectAnnouncementDrop;

        public GameObject LightEffect => lightEffect;
        public GameObject PerfectAnnouncementDrop => perfectAnnouncementDrop;
    }
}
