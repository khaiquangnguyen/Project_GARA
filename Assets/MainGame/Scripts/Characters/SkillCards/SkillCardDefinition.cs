using System;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters
{
    // Authored asset for one skill card: what it costs, what input minigame
    // gates it, and what effects it resolves once that minigame finishes.
    // The character state that plays it references the card
    // (CharacterState.SkillCard) — using the card means playing that state,
    // and the state owns timing, animation and on-hit. A card no state on
    // the character references is dropped from any combat loadout (see
    // CharacterDefinition.ResolveCombatLoadout). Used
    // as-is, a card has no minigame — it completes instantly with a
    // full-score performance and the state does the rest. Subclasses (per
    // input-system) override CreateInputSession to gate it behind one.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Skill Card", fileName = "SkillCard")]
    public class SkillCardDefinition : ScriptableObject
    {
        public string cardId;

        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;

        public SpecialTargetMode targetMode;

        [Tooltip("How many targets a Multi* mode hits. With repeat, the same character can take several of these; without, it's capped at how many in the pool are still alive.")]
        [ShowIf(nameof(IsMultiTarget))]
        [Min(1)]
        public int multiTargetCount = 2;

        private bool IsMultiTarget => targetMode.IsMulti();

        public int apCost;

        public int mpCost;

        [Tooltip("Clip played when the card resolves — a live card's perfect finale. Its range is where MoveInFrontOfEnemy stands.")]
        [Expandable]
        [Required]
        public AttackAnimationSpec animationSpec;

        [Tooltip("MoveInFrontOfEnemy stands at animationSpec's range.")]
        public ActionPositionMode positionMode;

        [HideIf(nameof(HasSharedTiering))]
        public SkillPerformanceTiering tiering = SkillPerformanceTiering.Default;

        [Expandable]
        public SkillEffectDefinition[] effects = Array.Empty<SkillEffectDefinition>();

        [Tooltip("Resolved by a live card's perfect finale instead of effects.")]
        [Expandable]
        public SkillEffectDefinition[] perfectEffects = Array.Empty<SkillEffectDefinition>();

        public bool refundOnAbort;

        public virtual ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new InstantSkillInputSession(new SkillPerformance(1f, TierFor(1f), false, null));
        }

        // True when a subclass takes its tiering from elsewhere (hides the field).
        protected virtual bool HasSharedTiering => false;

        protected virtual SkillPerformanceTiering Tiering => tiering;

        public SkillPerformanceTier TierFor(float score)
        {
            return Tiering.Evaluate(score);
        }

        // Completes the moment it begins.
        private class InstantSkillInputSession : ISkillInputSession
        {
            private readonly SkillPerformance _performance;

            public InstantSkillInputSession(SkillPerformance performance)
            {
                _performance = performance;
            }

            public void Begin(Action<SkillPerformance> onCompleted)
            {
                onCompleted?.Invoke(_performance);
            }

            public void Abort()
            {
            }
        }
    }
}
