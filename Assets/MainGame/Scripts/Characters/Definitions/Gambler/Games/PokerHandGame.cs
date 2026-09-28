using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GARA.Characters.Gambler
{
    // Deals 5 from a fresh deck. Cheat: extra hands are dealt and the best
    // is kept.
    [Serializable]
    public class PokerHandGame : GamblingGame<PokerHandOutcome>
    {
        [Tooltip("Cheat: extra hands dealt; the best one is kept.")]
        [FormerlySerializedAs("maxExtraDraws")]
        [Min(0)]
        public int cheatExtraDraws = 2;

        protected override PokerHandOutcome RollTyped(bool cheated, IGambleRandom rng)
        {
            var draws = 1 + (cheated ? cheatExtraDraws : 0);
            PlayingCard[] best = null;
            var bestRank = PokerHandRank.HighCard;
            for (var i = 0; i < draws; i++)
            {
                var hand = Deal(rng);
                var rank = PokerHandEvaluator.Evaluate(hand);
                if (best == null || rank > bestRank)
                {
                    best = hand;
                    bestRank = rank;
                }
            }

            return new PokerHandOutcome(best, bestRank);
        }

        // Partial Fisher-Yates over a 52-card deck.
        private static PlayingCard[] Deal(IGambleRandom rng)
        {
            var deck = new int[52];
            for (var i = 0; i < deck.Length; i++)
            {
                deck[i] = i;
            }

            var hand = new PlayingCard[PokerHandEvaluator.HandSize];
            for (var i = 0; i < hand.Length; i++)
            {
                var pick = rng.Range(i, deck.Length);
                (deck[i], deck[pick]) = (deck[pick], deck[i]);
                hand[i] = new PlayingCard(2 + deck[i] % 13, deck[i] / 13);
            }

            return hand;
        }
    }
}
