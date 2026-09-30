using System;
using GARA.SkillCards.Rhythm;

namespace GARA.Characters.Exorcist
{
    // Swaps every transferable status between the step's first two targets,
    // when its gate passes.
    [Serializable]
    public class SwapStatuses : GatedRhythmStepEffect
    {
        public override void ApplyEffect(in SkillEffectContext context)
        {
            if (context.Targets.Count < 2 || context.Targets[0] == context.Targets[1])
            {
                return;
            }

            var first = context.Targets[0];
            var second = context.Targets[1];
            var fromFirst = first.TakeStatuses(IsTransferable);
            var fromSecond = second.TakeStatuses(IsTransferable);
            foreach (var status in fromSecond)
            {
                first.ApplyStatus(status);
            }

            foreach (var status in fromFirst)
            {
                second.ApplyStatus(status);
            }
        }

        private static bool IsTransferable(StatusEffectInstance status) => status.kind.IsTransferable();
    }
}
