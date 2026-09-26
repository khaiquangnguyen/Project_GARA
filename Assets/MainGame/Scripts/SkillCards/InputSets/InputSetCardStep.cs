using System;
using GARA.Characters;
using GARA.InputSets;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.InputSets
{
    // One step of a live input-set card: its set, move and gated effects.
    [Serializable]
    public struct InputSetCardStep
    {
        public InputSetDefinition set;

        [Tooltip("Played once the set finishes, if any effect's gate passes. Empty = nothing plays for this step.")]
        [Expandable]
        public AttackAnimationSpec move;

        [Tooltip("Gated on this step's set; the triggered ones apply on the move's hit. Needs a move.")]
        [SerializeReference]
        [SubclassPicker]
        public InputSetStepEffect[] effects;
    }
}
