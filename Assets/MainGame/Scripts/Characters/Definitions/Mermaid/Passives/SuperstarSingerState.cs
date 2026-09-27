using System.Collections.Generic;

namespace GARA.Characters.Mermaid
{
    // Charm built up on each opponent.
    public sealed class SuperstarSingerState : PassiveRuntimeState
    {
        private readonly Dictionary<ICombatTarget, int> _charmByTarget = new Dictionary<ICombatTarget, int>();

        public int CharmOf(ICombatTarget target)
        {
            return _charmByTarget.TryGetValue(target, out var charm) ? charm : 0;
        }

        public int AddCharm(ICombatTarget target, int amount)
        {
            var charm = CharmOf(target) + amount;
            _charmByTarget[target] = charm;
            return charm;
        }

        public void ClearCharm(ICombatTarget target)
        {
            _charmByTarget.Remove(target);
        }

        public override void Reset()
        {
            _charmByTarget.Clear();
        }
    }
}
