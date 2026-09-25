using System;
using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    public enum SlotMatchKind
    {
        Any,
        AllReelsMatch,
        AtLeastNMatching,
        SymbolCount,
        ExactCombination
    }

    // Authored condition a SlotMachineRoll is checked against, to decide
    // which SlotMachineOutcomeEffect (if any) fires.
    [Serializable]
    public struct SlotMatchPattern
    {
        public SlotMatchKind kind;
        public int minimumMatchingReels;
        public string requiredSymbolId;
        public int requiredSymbolCount;
        public string[] exactSymbolIds;
        public bool ignoreOrder;

        public bool Matches(SlotMachineRoll roll, SlotMachineDefinition machine)
        {
            switch (kind)
            {
                case SlotMatchKind.Any:
                    return true;
                case SlotMatchKind.AllReelsMatch:
                    if (!roll.allMatch)
                    {
                        return false;
                    }
                    return string.IsNullOrEmpty(requiredSymbolId) || (roll.symbolIds.Count > 0 && roll.symbolIds[0] == requiredSymbolId);
                case SlotMatchKind.AtLeastNMatching:
                    return roll.largestMatchCount >= minimumMatchingReels;
                case SlotMatchKind.SymbolCount:
                    if (!machine.TryGetSymbolIndex(requiredSymbolId, out var idx))
                    {
                        return false;
                    }
                    return roll.CountOf(idx) >= requiredSymbolCount;
                case SlotMatchKind.ExactCombination:
                    return MatchesExact(roll);
                default:
                    return false;
            }
        }

        private readonly bool MatchesExact(SlotMachineRoll roll)
        {
            if (exactSymbolIds == null || exactSymbolIds.Length != roll.symbolIds.Count)
            {
                return false;
            }

            if (!ignoreOrder)
            {
                for (var i = 0; i < exactSymbolIds.Length; i++)
                {
                    if (exactSymbolIds[i] != roll.symbolIds[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            var remaining = new List<string>(roll.symbolIds);
            foreach (var required in exactSymbolIds)
            {
                if (!remaining.Remove(required))
                {
                    return false;
                }
            }

            return remaining.Count == 0;
        }
    }
}
