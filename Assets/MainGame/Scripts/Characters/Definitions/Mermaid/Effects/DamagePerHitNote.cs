using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Damage to every target for each note of the step that was hit.
    [Serializable]
    public class DamagePerHitNote : RhythmStepEffect
    {
        [Min(1)]
        public int damagePerNote = 8;

        public override bool IsTriggered(RhythmCompletionReport report)
        {
            return report.HitNotes > 0;
        }

        public override void ApplyEffect(in SkillEffectContext context)
        {
            if (!context.Performance.TryGetDetails<RhythmCompletionReport>(out var report))
            {
                return;
            }

            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damagePerNote * report.HitNotes);
            }
        }
    }
}
