using System.Collections.Generic;

namespace GARA.Characters.Chef
{
    // Mastery stacks per flavor, earned by filling up enemies.
    public sealed class MasterchefState : PassiveRuntimeState
    {
        private readonly Dictionary<FlavorTag, int> _masteryByFlavor = new Dictionary<FlavorTag, int>();

        public int MasteryOf(FlavorTag flavor)
        {
            return _masteryByFlavor.TryGetValue(flavor, out var stacks) ? stacks : 0;
        }

        public void AddMastery(FlavorTag flavor)
        {
            _masteryByFlavor[flavor] = MasteryOf(flavor) + 1;
        }

        public override void Reset()
        {
            _masteryByFlavor.Clear();
        }
    }
}
