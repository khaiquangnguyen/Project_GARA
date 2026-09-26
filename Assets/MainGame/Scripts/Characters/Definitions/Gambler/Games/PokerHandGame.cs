using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Deals 5 from a fresh deck; luck deals extra hands and keeps the best.
    [Serializable]
    public class PokerHandGame : GamblingGame<PokerHandOutcome>
    {
        [Tooltip("Extra hands dealt at full luck; the best one is kept.")]
        [Min(0)]
        public int maxExtraDraws = 2;

        protected override PokerHandOutcome RollTyped(float luck, IGambleRandom rng)
        {
            var draws = 1 + Mathf.RoundToInt(maxExtraDraws * luck);
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
