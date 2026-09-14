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

        public void ChangeState(CharacterState next, CharacterStateContext context)
        {
            if (next == null || next == CurrentState)
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = next;
            CurrentState.Enter(context);
        }
    }
}
