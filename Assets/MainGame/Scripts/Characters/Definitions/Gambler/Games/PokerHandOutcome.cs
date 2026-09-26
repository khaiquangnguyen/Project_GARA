using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    public sealed class PokerHandOutcome : GambleOutcome
    {
        public readonly IReadOnlyList<PlayingCard> Hand;
        public readonly PokerHandRank Rank;

        public PokerHandOutcome(IReadOnlyList<PlayingCard> hand, PokerHandRank rank)
        {
            Hand = hand;
            Rank = rank;
        }

        public override float Significance => (float)Rank / (float)PokerHandRank.RoyalFlush;

        public override string ToString()
        {
            return $"Poker: {Rank} [{string.Join(" ", Hand)}]";
        }
    }
}
