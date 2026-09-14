namespace GARA.Combat
{
    // Cosmetic/targeting-only positioning — deliberately carries no stat
    // modifiers. Just where a participant stands and what a target picker
    // can filter on.
    public class FormationSlot
    {
        public int index;
        public CombatParticipant occupant;

        public FormationSlot(int index)
        {
            this.index = index;
        }
    }
}
