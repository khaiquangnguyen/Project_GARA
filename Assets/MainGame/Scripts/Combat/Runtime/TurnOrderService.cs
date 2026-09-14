using System.Collections.Generic;
using System.Linq;
using GARA.Characters;

namespace GARA.Combat
{
    // Pure ordering logic — how a set of participants sorts into a turn
    // order (by current Speed). When and to whom that ordering gets applied
    // (a whole new group vs. just a group's not-yet-acted tail) is
    // TurnOrderGroup's job, not this class's.
    public static class TurnOrderService
    {
        public static List<CombatParticipant> SortBySpeed(IEnumerable<CombatParticipant> participants)
        {
            return participants
                .Where(p => p != null && !p.IsDefeated)
                .OrderByDescending(p => p.GetCurrentStats().Speed.Value)
                .ToList();
        }

        public static TurnOrderGroup CreateGroup(BattleContext context)
        {
            return new TurnOrderGroup(context.AllParticipants);
        }
    }
}
