using System;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Dancer passive: perfectly-executed skill cards (no misses, and
    // optionally requiring the Perfect performance tier) bank their hit
    // notes toward an encore threshold. Each time enough notes accumulate,
    // a bonus set of effects fires against encoreTargetMode, independent of
    // whatever the triggering card actually targeted.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Spotlight Lover", fileName = "SpotlightLoverPassive")]
    public class SpotlightLoverPassive : PassiveDefinition<SpotlightLoverState>
    {
        [SerializeField] private int encoreThreshold = 10;
        [SerializeField] private bool requirePerfectTier;
        [SerializeField] private SpecialTargetMode encoreTargetMode = SpecialTargetMode.AllEnemy;
        [SerializeField] private SkillEffectDefinition[] encoreEffects = Array.Empty<SkillEffectDefinition>();
        [SerializeField] private int maxEncoresPerTrigger = 8;

        public int EncoreThreshold => encoreThreshold;

        protected override void OnSkillCardResolved(in PassiveContext context, SpotlightLoverState state)
        {
            if (!context.Performance.TryGetDetails<RhythmCompletionReport>(out var report))
            {
                return;
            }

            if (context.Performance.WasAborted || report.WasAborted)
            {
                return;
            }

            if (!IsPerfectlyExecuted(report, context.Performance))
            {
                return;
            }

            if (report.HitNotes <= 0)
            {
                return;
            }

            state.encoreNotes += report.HitNotes;
            DrainEncores(in context, state);
        }

        private bool IsPerfectlyExecuted(RhythmCompletionReport report, in SkillPerformance performance)
        {
            return report.TotalNotes > 0
                && report.MissCount == 0
                && (!requirePerfectTier || performance.Tier == SkillPerformanceTier.Perfect);
        }

        private void DrainEncores(in PassiveContext context, SpotlightLoverState state)
        {
            if (encoreThreshold <= 0)
            {
                return;
            }

            var triggers = 0;
            while (state.encoreNotes >= encoreThreshold && triggers < maxEncoresPerTrigger)
            {
                state.encoreNotes -= encoreThreshold;
                state.encoresTriggered++;
                triggers++;
                PerformEncore(in context, state, triggers);
            }

            if (triggers >= maxEncoresPerTrigger && state.encoreNotes >= encoreThreshold)
            {
                Debug.LogWarning($"{name}: hit maxEncoresPerTrigger ({maxEncoresPerTrigger}) with encoreNotes still at {state.encoreNotes} — consider raising the threshold or the cap.", this);
            }
        }

        private void PerformEncore(in PassiveContext context, SpotlightLoverState state, int indexThisResolution)
        {
            var payload = new EncoreTriggerReport(indexThisResolution, state.encoresTriggered, state.encoreNotes, encoreThreshold, context.Card);
            var performance = new SkillPerformance(1f, SkillPerformanceTier.Perfect, false, payload);
            PassiveEffectRunner.Resolve(encoreEffects, in context, encoreTargetMode, performance);
        }
    }
}
