using System.Collections.Generic;
using System.Linq;
using GARA.Characters;

namespace GARA.Combat
{
    // One "lap" through the roster: every participant alive when the group
    // was formed takes exactly one turn, in Speed order — this is what
    // guarantees every character gets exactly one action per group. Order
    // is computed once at construction (or refreshed via
    // RecalculateRemaining), then only walked forward via AdvanceCursor as
    // each member's turn resolves; already-acted members never move, since
    // they're done for this group.
    public class TurnOrderGroup
    {
        public List<CombatParticipant> Order { get; private set; }
        public int Cursor { get; private set; }

        public TurnOrderGroup(IEnumerable<CombatParticipant> participants)
        {
            Order = TurnOrderService.SortBySpeed(participants);
            SkipDefeatedActors();
        }

        public CombatParticipant CurrentActor => Cursor < Order.Count ? Order[Cursor] : null;

        public bool IsExhausted => Cursor >= Order.Count;

        // Everyone in this group who hasn't acted yet, current actor first.
        public IEnumerable<CombatParticipant> RemainingInOrder => Order.Skip(Cursor);

        public void AdvanceCursor()
        {
            Cursor++;
            SkipDefeatedActors();
        }

        // A participant defeated before their turn comes up is skipped
        // rather than given a turn.
        private void SkipDefeatedActors()
        {
            while (Cursor < Order.Count && Order[Cursor].IsDefeated)
            {
                Cursor++;
            }
        }

        // Re-sorts only the not-yet-acted tail by current Speed —
        // already-acted members keep their position; anyone defeated since
        // the group was formed drops out of the remaining order.
        public void RecalculateRemaining()
        {
            var acted = Order.Take(Cursor).ToList();
            var remaining = TurnOrderService.SortBySpeed(Order.Skip(Cursor));
            Order = acted.Concat(remaining).ToList();
        }
    }
}
