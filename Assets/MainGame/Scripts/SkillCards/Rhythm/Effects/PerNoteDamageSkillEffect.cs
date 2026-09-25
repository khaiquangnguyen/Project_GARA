using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // Sums per-note damage weighted by judgement instead of a single
    // tier/score multiplier. Falls back to a generic score-scaled hit
    // (assuming a ~5-note baseline) when Performance.Details isn't a
    // RhythmCompletionReport, so this stays usable on a non-rhythm card too.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Rhythm/Per-Note Damage")]
    public class PerNoteDamageSkillEffect : SkillEffectDefinition
    {
        [SerializeField]
        private float damagePerNote;

        [SerializeField]
        private float perfectCritMultiplier = 1.5f;

        [SerializeField]
        private bool fullComboBonus = true;

        [SerializeField]
        private int fullComboBonusStrikes = 1;

        public override void Resolve(in SkillEffectContext context)
        {
            var damage = context.Performance.TryGetDetails<RhythmCompletionReport>(out var report)
                ? ComputeFromReport(report)
                : Mathf.RoundToInt(damagePerNote * 5f * context.Performance.Score);

            foreach (var target in context.Targets)
            {
                target.ApplyDamage(damage);
            }
        }

        private int ComputeFromReport(RhythmCompletionReport report)
        {
            var total = 0f;

            foreach (var result in report.NoteResults)
            {
                total += result.Judgement switch
                {
                    RhythmJudgement.Perfect => damagePerNote * perfectCritMultiplier,
                    RhythmJudgement.Good => damagePerNote * 0.75f,
                    RhythmJudgement.Ok => damagePerNote * 0.5f,
                    _ => 0f
                };
            }

            if (fullComboBonus && report.MissCount == 0)
            {
                total += fullComboBonusStrikes * damagePerNote;
            }

            return Mathf.RoundToInt(total);
        }
    }
}
