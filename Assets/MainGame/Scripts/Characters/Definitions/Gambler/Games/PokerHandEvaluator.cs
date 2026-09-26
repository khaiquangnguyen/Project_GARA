using System;
using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    public static class PokerHandEvaluator
    {
        public const int HandSize = 5;

        private const int Ace = 14;

        public static PokerHandRank Evaluate(IReadOnlyList<PlayingCard> hand)
        {
            if (hand == null || hand.Count != HandSize)
            {
                throw new ArgumentException($"A poker hand is {HandSize} cards.", nameof(hand));
            }

            var ranks = new int[HandSize];
            var flush = true;
            for (var i = 0; i < HandSize; i++)
            {
                ranks[i] = hand[i].Rank;
                flush &= hand[i].Suit == hand[0].Suit;
            }

            Array.Sort(ranks);
            var straight = IsStraight(ranks);

            if (straight && flush)
            {
                return ranks[0] == 10 ? PokerHandRank.RoyalFlush : PokerHandRank.StraightFlush;
            }

            // Group sizes, largest first (e.g. full house = 3,2).
            var groups = new List<int>();
            var run = 1;
            for (var i = 1; i <= HandSize; i++)
            {
                if (i < HandSize && ranks[i] == ranks[i - 1])
                {
                    run++;
                    continue;
                }

                groups.Add(run);
                run = 1;
            }

            groups.Sort((a, b) => b.CompareTo(a));

            if (groups[0] == 4)
            {
                return PokerHandRank.FourOfAKind;
            }

            if (groups[0] == 3 && groups[1] == 2)
            {
                return PokerHandRank.FullHouse;
            }

            if (flush)
            {
                return PokerHandRank.Flush;
            }

            if (straight)
            {
                return PokerHandRank.Straight;
            }

            if (groups[0] == 3)
            {
                return PokerHandRank.ThreeOfAKind;
            }

            if (groups[0] == 2)
            {
                return groups[1] == 2 ? PokerHandRank.TwoPair : PokerHandRank.OnePair;
            }

            return PokerHandRank.HighCard;
        }

        // Ranks sorted ascending; A-2-3-4-5 counts.
        private static bool IsStraight(int[] ranks)
        {
            var wheel = ranks[0] == 2 && ranks[1] == 3 && ranks[2] == 4 && ranks[3] == 5 && ranks[4] == Ace;
            if (wheel)
            {
                return true;
            }

            for (var i = 1; i < ranks.Length; i++)
            {
                if (ranks[i] != ranks[i - 1] + 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
