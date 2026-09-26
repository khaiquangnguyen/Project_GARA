using System.Collections.Generic;

namespace GARA.Characters.Noir
{
    // Clues stacked on each opponent, plus the Case File every clue also fills.
    public sealed class NoirState : PassiveRuntimeState
    {
        private readonly Dictionary<ICombatTarget, int> _cluesByTarget = new Dictionary<ICombatTarget, int>();

        public int CaseFile { get; private set; }

        public int CluesOn(ICombatTarget target)
        {
            return _cluesByTarget.TryGetValue(target, out var clues) ? clues : 0;
        }

        public int AddClues(ICombatTarget target, int amount)
        {
            var clues = CluesOn(target) + amount;
            _cluesByTarget[target] = clues;
            return clues;
        }

        public void AddToCaseFile(int amount)
        {
            CaseFile += amount;
        }

        public void CloseCaseFile()
        {
            CaseFile = 0;
        }

        public override void Reset()
        {
            _cluesByTarget.Clear();
            CaseFile = 0;
        }
    }
}
