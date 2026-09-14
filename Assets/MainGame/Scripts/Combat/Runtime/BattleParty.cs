using System.Collections.Generic;
using System.Linq;

namespace GARA.Combat
{
    // Fixed-size 3-slot party for this game's 3v3 format.
    public class BattleParty
    {
        public const int Size = 3;

        public FormationSlot[] Slots { get; } = new FormationSlot[Size];

        public BattleParty()
        {
            for (var i = 0; i < Size; i++)
            {
                Slots[i] = new FormationSlot(i);
            }
        }

        public CombatParticipant GetBySlot(int index) => Slots[index].occupant;

        public IEnumerable<CombatParticipant> LivingMembers()
        {
            return Slots
                .Where(slot => slot.occupant != null && !slot.occupant.IsDefeated)
                .Select(slot => slot.occupant);
        }

        public bool IsWiped() => !LivingMembers().Any();
    }
}
