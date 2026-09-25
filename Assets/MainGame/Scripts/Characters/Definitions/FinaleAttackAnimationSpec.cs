using UnityEngine;

namespace GARA.Characters
{
    // A live card's perfect-finale clip: an attack spec plus the
    // announcement dropped onto each target, landing on its hit frame.
    [CreateAssetMenu(menuName = "GARA/Characters/Finale Attack Animation Spec", fileName = "FinaleAttackAnimationSpec")]
    public class FinaleAttackAnimationSpec : AttackAnimationSpec
    {
        [Tooltip("Prefab (PerfectAnnouncementDropEffect at its root) dropped onto each target on the perfect finale.")]
        [SerializeField]
        private GameObject perfectAnnouncementDrop;

        public GameObject PerfectAnnouncementDrop => perfectAnnouncementDrop;
    }
}
