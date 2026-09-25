using System;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters
{
    // Authored asset for one skill card: what it costs, what animation state
    // plays, what input minigame gates it, and what effects it resolves once
    // that minigame finishes. Concrete subclasses (per input-system) provide
    // CreateInputSession; everything else is shared.
    public abstract class SkillCardDefinition : ScriptableObject
    {
        public string cardId;

        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;

        public SpecialTargetMode targetMode;

        public int apCost;

        public int mpCost;

        public CharacterState animationState;

        public ActionPositionMode positionMode;

        public SkillPerformanceTiering tiering = SkillPerformanceTiering.Default;

        [Expandable]
        public SkillEffectDefinition[] effects = Array.Empty<SkillEffectDefinition>();

        public bool refundOnAbort;

        public abstract ISkillInputSession CreateInputSession(ISkillInputHost host);

        public SkillPerformanceTier TierFor(float score)
        {
            return tiering.Evaluate(score);
        }
    }
}
