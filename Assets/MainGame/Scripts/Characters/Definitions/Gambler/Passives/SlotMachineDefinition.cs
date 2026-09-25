using System;
using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Authored slot machine: a reel count and a weighted symbol pool.
    // GamblingAddictPassive rolls this whenever the accumulated chips drain
    // past the chip threshold.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Slot Machine", fileName = "SlotMachine")]
    public class SlotMachineDefinition : ScriptableObject
    {
        [SerializeField]
        private int reelCount = 3;

        [SerializeField]
        private SlotSymbol[] symbols = Array.Empty<SlotSymbol>();

        public int ReelCount => reelCount;
        public IReadOnlyList<SlotSymbol> Symbols => symbols;

        public SlotMachineRoll Roll(IGambleRandom rng)
        {
            var indices = new int[reelCount];
            var ids = new string[reelCount];
            var counts = new Dictionary<int, int>();
            for (var reel = 0; reel < reelCount; reel++)
            {
                var index = PickWeightedIndex(rng);
                indices[reel] = index;
                ids[reel] = index >= 0 && index < symbols.Length ? symbols[index].symbolId : null;
                counts.TryGetValue(index, out var c);
                counts[index] = c + 1;
            }

            var largestCount = 0;
            var largestSymbol = -1;
            foreach (var kv in counts)
            {
                if (kv.Value > largestCount)
                {
                    largestCount = kv.Value;
                    largestSymbol = kv.Key;
                }
            }

            var allMatch = reelCount > 0 && largestCount == reelCount;
            return new SlotMachineRoll(indices, ids, allMatch, largestCount, largestSymbol);
        }

        private int PickWeightedIndex(IGambleRandom rng)
        {
            if (symbols.Length == 0)
            {
                return -1;
            }

            var totalWeight = 0f;
            foreach (var s in symbols)
            {
                totalWeight += s.weight > 0f ? s.weight : 1f;
            }

            var roll = rng.Value01() * totalWeight;
            var cumulative = 0f;
            for (var i = 0; i < symbols.Length; i++)
            {
                cumulative += symbols[i].weight > 0f ? symbols[i].weight : 1f;
                if (roll <= cumulative)
                {
                    return i;
                }
            }

            return symbols.Length - 1;
        }

        public bool TryGetSymbolIndex(string symbolId, out int index)
        {
            for (var i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].symbolId == symbolId)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}
