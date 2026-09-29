using System;
using System.Collections.Generic;
using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // Rolls and spins as they happen, for displays (GambleOutcomeView) to
    // show. Raised by the Gambler's sessions and Slot Machine passive.
    public static class GambleEvents
    {
        // The card's game was just rolled.
        public static event Action<GamblerSkillCard, GambleOutcome, bool> Rolled;

        // The Slot Machine passive's reels just stopped.
        public static event Action<ICombatTarget, IReadOnlyList<SlotSymbol>, bool> SlotSpun;

        internal static void RaiseRolled(GamblerSkillCard card, GambleOutcome outcome, bool cheated)
        {
            Rolled?.Invoke(card, outcome, cheated);
        }

        internal static void RaiseSlotSpun(ICombatTarget spinner, IReadOnlyList<SlotSymbol> spin, bool jackpot)
        {
            SlotSpun?.Invoke(spinner, spin, jackpot);
        }
    }
}
