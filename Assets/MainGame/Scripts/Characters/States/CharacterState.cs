using System;
using UnityEngine;

namespace GARA.Characters
{
    // Base type for anything a character can be doing: an attack, a jump, a
    // dodge, a defend, etc. Declared here (not in GARA.Combat) so
    // BasicAttackEntry/ComboDefinition can reference it without
    // GARA.Characters depending on GARA.Combat — concrete states live in
    // GARA.Combat and are free to use real combat types.
    //
    // Assumption: at most one subclass of a given concrete type anywhere
    // under the character prefab's root. Resolution
    // (CombatParticipant.ResolveCharacterStates) uses
    // GetComponentInChildren(type), which is ambiguous otherwise. States
    // don't need to live on the root itself — commonly they're grouped
    // under a child (e.g. "States") — but each concrete type must still be
    // unique within the whole hierarchy.
    public abstract class CharacterState : MonoBehaviour
    {
        // Raised by the subclass whenever it considers itself done — e.g. a
        // SpineAnimationState raises this off the animation's own Complete
        // event, not a hand-authored duration.
        public event Action Finished;

        public abstract void Enter(CharacterStateContext context);

        public virtual void Exit()
        {
        }

        protected void RaiseFinished()
        {
            Finished?.Invoke();
        }
    }
}
