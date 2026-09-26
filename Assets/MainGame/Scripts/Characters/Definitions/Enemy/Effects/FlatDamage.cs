using System;
using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Enemy
{
    [Serializable]
    public class FlatDamage : EnemyEffect
    {
        [Min(0)]
        public int damage = 5;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }
    }
}
