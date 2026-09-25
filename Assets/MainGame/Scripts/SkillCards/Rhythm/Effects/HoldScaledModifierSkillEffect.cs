using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // Timed modifier whose duration gets a bonus from how much of the
    // sequence's notes were fully completed. RhythmNoteResult doesn't carry
    // its source note's RhythmNoteKind, so Tap and completed Hold notes are
    // indistinguishable here - both report HoldCompletion == 1 when hit. The
    // bonus is computed from the summed HoldCompletion across all results
    // (see RhythmNoteResult.HoldCompletion docs: "Always 1 for Tap notes"),
    // which rewards a clean full-combo run generally rather than isolating
    // Hold notes specifically. Falls back to just baseDurationTurns when no
    // rhythm report is available.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Effects/Rhythm/Hold-Scaled Modifier")]
    public class HoldScaledModifierSkillEffect : SkillEffectDefinition
    {
        [SerializeField]
        private StatKind stat;

        [SerializeField]
        private float magnitude;

        [SerializeField]
        private int baseDurationTurns = 1;

        [SerializeField]
        private int maxBonusTurns = 4;

        [SerializeField]
        private bool applyToSelfInsteadOfTargets;

        public override void Resolve(in SkillEffectContext context)
        {
            var duration = baseDurationTurns;

            if (context.Performance.TryGetDetails<RhythmCompletionReport>(out var report))
            {
                var holdScore = 0f;
                foreach (var result in report.NoteResults)
                {
                    holdScore += result.HoldCompletion;
                }

                duration += Mathf.Min(maxBonusTurns, Mathf.RoundToInt(holdScore));
            }

            if (applyToSelfInsteadOfTargets)
            {
                context.Self?.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, duration, name));
                return;
            }

            foreach (var target in context.Targets)
            {
                target.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, duration, name));
            }
        }
    }
}
