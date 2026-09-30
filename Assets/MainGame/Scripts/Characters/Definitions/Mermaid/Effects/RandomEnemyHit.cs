using System;
using System.Linq;
using GARA.SkillCards.Rhythm;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GARA.Characters.Mermaid
{
    // Hits one random pickable opponent among the targets (see
    // TargetRestrictions).
    [Serializable]
    public class RandomEnemyHit : GatedRhythmStepEffect
    {
        [Min(0)]
        public int damage = 8;

        public override void ApplyEffect(in SkillEffectContext context)
        {
            var self = context.Self;
            var opponents = context.Targets.Where(target => !target.IsDefeated && target.Faction != self.Faction);
            var pool = TargetRestrictions.Pickable(self, opponents, true, out _);
            if (pool.Count == 0)
            {
                return;
            }

            pool[Random.Range(0, pool.Count)].ApplyDamage(damage);
        }
    }
}
