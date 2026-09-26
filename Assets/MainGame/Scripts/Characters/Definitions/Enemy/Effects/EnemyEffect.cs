using System;
using GARA.Characters;

namespace GARA.Characters.Enemy
{
    // An effect on an enemy card's hit; always applies.
    [Serializable]
    public abstract class EnemyEffect : ISkillEffect
    {
        public abstract void ApplyEffect(in SkillEffectContext context);
    }
}
