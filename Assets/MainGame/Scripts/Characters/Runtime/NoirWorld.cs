using System;

namespace GARA.Characters
{
    // Battle-wide Noir World: once entered, every character on both sides is
    // inside it for a number of turns. One per battle, owned by the combat
    // side; passives reach it through IBattleQuery.NoirWorld.
    public sealed class NoirWorld
    {
        public bool IsActive { get; private set; }
        public int RemainingTurns { get; private set; }

        // Whoever pulled everyone in; null while inactive.
        public ICombatTarget Source { get; private set; }

        public event Action<NoirWorld> Entered;
        public event Action<NoirWorld> Exited;

        // No-op while already active — the world never stacks or refreshes.
        public bool TryEnter(ICombatTarget source, int turns)
        {
            if (IsActive || turns <= 0)
            {
                return false;
            }

            IsActive = true;
            RemainingTurns = turns;
            Source = source;
            Entered?.Invoke(this);
            return true;
        }

        // Called once as each turn ends; exits when the last turn runs out.
        public void TickTurn()
        {
            if (!IsActive)
            {
                return;
            }

            RemainingTurns--;
            if (RemainingTurns <= 0)
            {
                Exit();
            }
        }

        public void Exit()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            RemainingTurns = 0;
            Source = null;
            Exited?.Invoke(this);
        }
    }
}
