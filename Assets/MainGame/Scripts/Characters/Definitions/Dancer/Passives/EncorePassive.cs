using GARA.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Dancer passive: each Perfect note adds a stack. Reaching the threshold
    // spends it and grants one extra turn after the current turn ends.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Encore", fileName = "EncorePassive")]
    public class EncorePassive : PassiveDefinition<EncoreState>
    {
        [Tooltip("Stacks spent to earn an extra turn. At most one extra turn per turn; leftover stacks carry over.")]
        [SerializeField] private int stackThreshold = 10;

        public int StackThreshold => stackThreshold;

        protected override void OnSkillCardResolved(in PassiveContext context, EncoreState state)
        {
            if (!context.Performance.TryGetDetails<RhythmCompletionReport>(out var report))
            {
                return;
            }

            if (context.Performance.WasAborted || report.WasAborted)
            {
                return;
            }

            state.stacks += report.PerfectCount;

            if (!state.extraTurnPending && stackThreshold > 0 && state.stacks >= stackThreshold)
            {
                state.stacks -= stackThreshold;
                state.extraTurnPending = true;
            }
        }

        protected override bool TryConsumeExtraTurn(EncoreState state)
        {
            if (!state.extraTurnPending)
            {
                return false;
            }

            state.extraTurnPending = false;
            return true;
        }
    }
}
