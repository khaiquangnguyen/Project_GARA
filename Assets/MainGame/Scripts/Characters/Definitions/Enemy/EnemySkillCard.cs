using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Enemy
{
    // No minigame and no finale: playing it plays animationSpec once, and
    // every effect applies on its hit.
    [CreateAssetMenu(menuName = "GARA/Characters/Enemy/Enemy Card", fileName = "EnemyCard")]
    public class EnemySkillCard : SkillCardDefinition
    {
        [SerializeReference]
        [SubclassPicker]
        public EnemyEffect[] hitEffects = Array.Empty<EnemyEffect>();

        protected override bool HasCardEffects => false;

        protected override bool HasPerfectEffects => false;

        // Every hit of an enemy's clip lands, multi-hit or not.
        public override bool ImpactOnEveryHit => true;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new EnemyLiveSkillInputSession(this);
        }

        protected virtual void OnValidate()
        {
            if (animationSpec == null)
            {
                Debug.LogWarning($"{name}: EnemySkillCard has no animationSpec — it never plays.", this);
            }

            if (hitEffects.Length == 0)
            {
                Debug.LogWarning($"{name}: EnemySkillCard has no hit effects — it never plays.", this);
            }
        }
    }
}
