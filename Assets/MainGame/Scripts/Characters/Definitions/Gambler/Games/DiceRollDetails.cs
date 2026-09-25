namespace GARA.Characters.Gambler
{
    public readonly struct DiceRollDetails
    {
        public readonly int[] Rolls;
        public readonly int Kept;
        public readonly int Goal;

        public DiceRollDetails(int[] rolls, int kept, int goal)
        {
            Rolls = rolls;
            Kept = kept;
            Goal = goal;
        }
    }
}
