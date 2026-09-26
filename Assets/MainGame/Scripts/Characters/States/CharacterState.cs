using System;
using UnityEngine;

namespace GARA.Characters
{
    // Base type for anything a character can be doing: an attack, a jump, a
    // dodge, a defend, etc. Declared here (not in GARA.Combat) so
    // CharacterDefinition can reference it without GARA.Characters depending
    // on GARA.Combat — concrete states live in GARA.Combat and are free to
    // use real combat types.
    //
    // A state that plays a skill card (a SkillCardState) references it and is
    // resolved by it. Any other state is resolved by concrete type
    // (CombatParticipant.ResolveCharacterStates), so at most one of each such
    // type may live under the character prefab's root.
    public abstract class CharacterState : MonoBehaviour
    {
        // The skill card that plays this state, if any.
        public virtual SkillCardDefinition SkillCard => null;

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
