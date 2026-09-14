using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // Owns which CharacterState a character is currently in. Lives on the
    // battle prefab alongside CharacterDefinition and every CharacterState
    // component. Attacks are the only thing driving state changes today
    // (via AttackExecutor), but this is the seam jump/dodge/defend/etc.
    // will plug into later — anything can call ChangeState given a live
    // CharacterState and a context.
    public class CharacterStateMachine : MonoBehaviour
    {
        public CharacterState CurrentState { get; private set; }

        // Re-entering the same state instance (e.g. the same swing played
        // twice in a row with nothing distinct in between, such as skipping
        // MoveForwardState because the actor's already at the target) is a
        // real case, not a no-op — Exit then Enter again so it actually
        // replays.
        public void ChangeState(CharacterState next, CharacterStateContext context)
        {
            if (next == null)
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = next;
            CurrentState.Enter(context);
        }
    }
}
