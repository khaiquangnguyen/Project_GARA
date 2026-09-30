using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

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

        [Tooltip("Live cards: which picks each step hits. The finale is set separately (finaleTargeting).")]
        [ShowIf(nameof(HasSeveralTargets))]
        [FormerlySerializedAs("stepsTakeTurnsAmongTargets")]
        public StepTargeting stepTargeting;

        [Tooltip("Live cards: which picks the finale hits.")]
        [ShowIf(nameof(HasSeveralTargets))]
        [FormerlySerializedAs("finaleHitsOneRandomPick")]
        public FinaleTargeting finaleTargeting;

        [Tooltip("Live cards: the user's allies step off stage for the card, then come back for the finale and dance along (a random clip each, until real dance clips exist).")]
        [BoxGroup(FinaleGroup)]
        [ShowIf(nameof(IsLive))]
        public bool alliesJoinFinale;

        [Tooltip("Seconds from the finale starting that the returning allies dance (random clips back to back), cut off when it runs out.")]
        [BoxGroup(FinaleGroup)]
        [ShowIf(nameof(alliesJoinFinale))]
        [Min(0f)]
        public float allyDanceDuration = 1f;

        [Tooltip("Live cards: each step snaps in front of its target instead of walking there — for steps too fast to walk between.")]
        [ShowIf(nameof(IsLive))]
        public bool teleportBetweenSteps;

        private bool IsMultiTarget => targetMode.IsMulti();

        private bool HasSeveralTargets => targetMode.IsMulti() || targetMode.IsAll();

        public int apCost;

        public int mpCost;

        [Tooltip("Filmmaker: perfect parries of this skill needed to record a copy of it.")]
        [Min(1)]
        public int parriesToRecord = 1;

        [Tooltip("MoveInFrontOfEnemy stands at animationSpec's range; StayAtOriginalPosition returns to the actor's own spot first. On a live card this covers its bars; the finale uses finalePositionMode.")]
        public ActionPositionMode positionMode;

        [ShowIf(nameof(HasCardEffects))]
        [Expandable]
        public SkillEffectDefinition[] effects = Array.Empty<SkillEffectDefinition>();

        public bool refundOnAbort;

        [Tooltip("Optional. Prefab (EncoreSpotlightEffect at its root) whose beams sweep onto the targets the moment the card is used, flaring on its finale's hit (or when it ends).")]
        public GameObject spotlight;

        [Tooltip("Optional. Prefab (ShadowScreenEffect at its root) that blacks out the stage and silhouettes the user on a lit screen the moment the card is used, until its finale's hit (or it ends).")]
        public GameObject shadowScreen;

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

        [Tooltip("Seconds between the last step's swing finishing and the finale starting, so the actor can reposition or animate first.")]
        [BoxGroup(FinaleGroup)]
        [ShowIf(nameof(IsLive))]
        [Min(0f)]
        public float finaleDelay = 0.5f;

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

        // True when the card leaves its user's hand once played (e.g. a
        // FormReplaySkillCard).
        public virtual bool IsOneTimeUse => false;

        // True when every "hit" event of the card's clips lands an impact,
        // whatever the spec's own impactOnEveryHit says.
        public virtual bool ImpactOnEveryHit => false;

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
