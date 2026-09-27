using System.Collections.Generic;

namespace GARA.Characters.Filmmaker
{
    // Recording stacks on each opponent.
    public sealed class RecordingState : PassiveRuntimeState
    {
        private readonly Dictionary<ICombatTarget, int> _stacksByTarget = new Dictionary<ICombatTarget, int>();

        public int StacksOn(ICombatTarget target)
        {
            return _stacksByTarget.TryGetValue(target, out var stacks) ? stacks : 0;
        }

        public void SetStacks(ICombatTarget target, int stacks)
        {
            _stacksByTarget[target] = stacks;
        }

        public override void Reset()
        {
            _stacksByTarget.Clear();
        }
    }
}
