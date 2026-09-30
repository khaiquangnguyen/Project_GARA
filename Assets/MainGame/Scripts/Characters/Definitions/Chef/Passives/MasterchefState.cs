using System;

namespace GARA.Characters.Chef
{
    // The Chef's satiety bar and the Masterchef stacks it has earned.
    public sealed class MasterchefState : PassiveRuntimeState
    {
        public int Satiety { get; private set; }

        public int Stacks { get; private set; }

        // True when this fills the bar; the overflow carries over, short of
        // a second fill.
        public bool AddSatiety(int amount, int capacity)
        {
            Satiety += amount;
            if (Satiety < capacity)
            {
                return false;
            }

            Satiety = Math.Min(Satiety - capacity, capacity - 1);
            Stacks++;
            return true;
        }

        public override void Reset()
        {
            Satiety = 0;
            Stacks = 0;
        }
    }
}
