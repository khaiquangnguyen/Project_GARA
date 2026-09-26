using System.Collections.Generic;
using System.Linq;
using GARA.Characters;

namespace GARA.Combat
{
    // Owns the state of one 3v3 battle. Turn order is modeled as two
    // TurnOrderGroups running back to back — "current" (in progress) and
    // "next" (fully pre-calculated ahead of time) — so the UI can always
    // preview upcoming turns past the end of the current group, and so a
    // future turn-order-altering effect can re-sort each group's own
    // remaining members independently without one group's changes bleeding
    // into the other's. Every participant alive when a group forms gets
    // exactly one turn in it. Reaction/interrupt handling (PendingAction
    // vs. resolved, interrupt stack) is deferred until that system is
    // designed — for now turns resolve in the order they're drawn.
    public class BattleContext
    {
        public BattleParty playerParty = new();
        public BattleParty enemyParty = new();

        public NoirWorld NoirWorld { get; } = new();

        public TurnOrderGroup CurrentGroup { get; private set; }
        public TurnOrderGroup NextGroup { get; private set; }

        public IEnumerable<CombatParticipant> AllParticipants =>
            playerParty.Slots.Select(s => s.occupant)
                .Concat(enemyParty.Slots.Select(s => s.occupant))
                .Where(p => p != null);

        public CombatParticipant CurrentActor => CurrentGroup?.CurrentActor;

        // Forms both groups fresh — called once, at battle start.
        public void InitializeTurnOrder()
        {
            CurrentGroup = TurnOrderService.CreateGroup(this);
            NextGroup = TurnOrderService.CreateGroup(this);
        }

        // Call once the current actor's turn has fully resolved. Rolls the
        // next group into place (and forms a fresh next group) the instant
        // the current one runs out of members who haven't acted yet.
        public void AdvanceTurn()
        {
            CurrentGroup.AdvanceCursor();
            if (CurrentGroup.IsExhausted)
            {
                CurrentGroup = NextGroup;
                NextGroup = TurnOrderService.CreateGroup(this);
            }
        }

        // Hook for a future turn-order-altering effect (a haste/slow,
        // etc.) — re-sorts each group's own not-yet-acted members
        // independently, never mixing the two groups together.
        public void RecalculateTurnOrder()
        {
            CurrentGroup?.RecalculateRemaining();
            NextGroup?.RecalculateRemaining();
        }

        // Up to `count` upcoming actors for display: whoever's left in the
        // current group (current actor first), then the whole next group.
        public List<CombatParticipant> GetUpcomingQueue(int count)
        {
            var queue = new List<CombatParticipant>();

            if (CurrentGroup != null)
            {
                queue.AddRange(CurrentGroup.RemainingInOrder);
            }

            if (NextGroup != null)
            {
                queue.AddRange(NextGroup.Order);
            }

            return queue.Take(count).ToList();
        }

        // Over once either side has no one left fighting for it (a charmed
        // enemy fights for the players).
        public bool IsBattleOver => !AnyLivingFightingFor(FactionTag.Player) || !AnyLivingFightingFor(FactionTag.Enemy);

        private bool AnyLivingFightingFor(FactionTag side)
        {
            return AllParticipants.Any(p => !p.IsDefeated && p.Allegiance == side);
        }

        public IBattleQuery QueryFor(CombatParticipant self)
        {
            return new BattleQuery(this, self);
        }
    }
}
