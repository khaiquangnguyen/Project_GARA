using UnityEngine;

namespace GARA.Characters
{
    // Flat damage to every target.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Flat Damage", fileName = "FlatDamageSkillEffect")]
    public class FlatDamageSkillEffect : SkillEffectDefinition
    {
        [Min(1)]
        public int damage = 10;

        public override void Resolve(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
