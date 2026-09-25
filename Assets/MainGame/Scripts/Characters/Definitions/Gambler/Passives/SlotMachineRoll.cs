using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    // Result of one SlotMachineDefinition.Roll call — the symbol landed on
    // by each reel, plus precomputed match info SlotMatchPattern reads.
    public sealed class SlotMachineRoll
    {
        public readonly IReadOnlyList<int> symbolIndices;
        public readonly IReadOnlyList<string> symbolIds;
        public readonly bool allMatch;
        public readonly int largestMatchCount;
        public readonly int largestMatchSymbolIndex;

        public SlotMachineRoll(IReadOnlyList<int> symbolIndices, IReadOnlyList<string> symbolIds, bool allMatch, int largestMatchCount, int largestMatchSymbolIndex)
        {
            this.symbolIndices = symbolIndices;
            this.symbolIds = symbolIds;
            this.allMatch = allMatch;
            this.largestMatchCount = largestMatchCount;
            this.largestMatchSymbolIndex = largestMatchSymbolIndex;
        }

        public int CountOf(int symbolIndex)
        {
            var count = 0;
            foreach (var i in symbolIndices)
            {
                if (i == symbolIndex)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
