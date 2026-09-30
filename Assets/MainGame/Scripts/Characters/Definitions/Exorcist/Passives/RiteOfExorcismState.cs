using System.Collections.Generic;

namespace GARA.Characters.Exorcist
{
    // Seals built up on each opponent.
    public sealed class RiteOfExorcismState : PassiveRuntimeState
    {
        private readonly Dictionary<ICombatTarget, int> _sealsByTarget = new Dictionary<ICombatTarget, int>();

        public int SealsOf(ICombatTarget target)
        {
            return _sealsByTarget.TryGetValue(target, out var seals) ? seals : 0;
        }

        public int AddSeals(ICombatTarget target, int amount)
        {
            var seals = SealsOf(target) + amount;
            _sealsByTarget[target] = seals;
            return seals;
        }

        public void ClearSeals(ICombatTarget target)
        {
            _sealsByTarget.Remove(target);
        }

        public override void Reset()
        {
            _sealsByTarget.Clear();
        }
    }
}
