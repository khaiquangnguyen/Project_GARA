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

        [Tooltip("MoveInFrontOfEnemy stands at animationSpec's range; StayAtOriginalPosition returns to the actor's own spot first. On a live card this covers its bars; the finale uses finalePositionMode.")]
        public ActionPositionMode positionMode;

        [ShowIf(nameof(HasCardEffects))]
        [Expandable]
        public SkillEffectDefinition[] effects = Array.Empty<SkillEffectDefinition>();

        public bool refundOnAbort;

        [Tooltip("Seconds the actor holds after the card's last swing before walking back, so the attack doesn't end abruptly.")]
        [Min(0f)]
        public float endDelay = 0.3f;

        [Tooltip("Clip played when the card resolves — a live card's perfect finale. Its range is where MoveInFrontOfEnemy stands.")]
        [BoxGroup(FinaleGroup)]
        [Expandable]
        [Required]
        public AttackAnimationSpec animationSpec;

        [Tooltip("Where a live card's finale plays, regardless of where its bars left the actor. MoveInFrontOfEnemy dashes in to animationSpec's range; StayAtOriginalPosition returns to the actor's own spot first.")]
        [BoxGroup(FinaleGroup)]
        [ShowIf(nameof(IsLive))]
        public ActionPositionMode finalePositionMode;

        [Tooltip("Resolved by a live card's perfect finale instead of effects.")]
        [BoxGroup(FinaleGroup)]
        [ShowIf(nameof(HasPerfectEffects))]
        [Expandable]
        public SkillEffectDefinition[] perfectEffects = Array.Empty<SkillEffectDefinition>();

        protected const string FinaleGroup = "Finale";

        public virtual ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new InstantSkillInputSession(new SkillPerformance(1f, SkillPerformanceTier.Perfect, false, null));
        }

        // True when the card plays moves as its input runs and ends on a
        // finale (an ILiveSkillInputSession); shows finalePositionMode.
        protected virtual bool IsLive => false;

        // False when a subclass resolves effects authored elsewhere (e.g. per
        // rhythm bar); hides effects.
        protected virtual bool HasCardEffects => true;

        // False when a subclass authors its own finale effects; hides
        // perfectEffects.
        protected virtual bool HasPerfectEffects => true;

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
