using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.InputSets;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.InputSets
{
    // Live input-set card: each step's set plays its move when finished; the
    // finale (animationSpec + finaleEffects) is gated on the whole run.
    // Scoring, tiering and retries come from the character using it.
    public abstract class LiveInputSetSkillCard : SkillCardDefinition
    {
        [SerializeField]
        private InputSetCardStep[] steps = Array.Empty<InputSetCardStep>();

        [Tooltip("Seconds for the whole run; 0 = no limit.")]
        [SerializeField]
        [Min(0)]
        private float totalTimeLimit;

        [Tooltip("Gated on the whole run; the triggered ones apply on the finale's hit. None triggered = no finale.")]
        [BoxGroup(FinaleGroup)]
        [SerializeReference]
        [SubclassPicker]
        public InputSetStepEffect[] finaleEffects = Array.Empty<InputSetStepEffect>();

        [NonSerialized]
        private InputSetCollectionDefinition _collection;

        // The set collection of the current (or last) use.
        public InputSetCollectionDefinition Collection => _collection;
        public IReadOnlyList<InputSetCardStep> Steps => steps;

        protected override bool IsLive => true;

        protected override bool HasCardEffects => false;

        protected override bool HasPerfectEffects => false;

        public AttackAnimationSpec OpeningMove
        {
            get
            {
                foreach (var step in steps)
                {
                    if (step.move != null)
                    {
                        return step.move;
                    }
                }

                return null;
            }
        }

        protected abstract SkillPerformanceTiering TieringFor(CharacterDefinition actor);

        protected abstract InputSetScoreModel ScoreModelFor(CharacterDefinition actor);

        protected abstract InputSetRetryPolicy RetryPolicyFor(CharacterDefinition actor);

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            if (_collection != null)
            {
                Destroy(_collection);
            }

            var sets = Array.ConvertAll(steps, step => step.set);
            _collection = InputSetCollectionDefinition.CreateRuntime(sets, totalTimeLimit, RetryPolicyFor(host.Actor));
            var player = host.GetDriver<InputSetCollectionPlayer>();
            return new InputSetLiveSkillInputSession(player, this, TieringFor(host.Actor), ScoreModelFor(host.Actor));
        }

        // Report covers one step's set or the whole run.
        protected internal virtual SkillPerformance BuildPerformance(InputSetCompletionReport report, SkillPerformanceTiering tiering, InputSetScoreModel scoreModel)
        {
            var score = scoreModel.Evaluate(report);
            var tier = report.WasAborted ? SkillPerformanceTier.Miss : tiering.Evaluate(score);
            return new SkillPerformance(score, tier, report.WasAborted, report);
        }

        protected virtual void OnValidate()
        {
            if (steps.Length == 0)
            {
                Debug.LogWarning($"{name}: LiveInputSetSkillCard has no steps.", this);
            }

            for (var s = 0; s < steps.Length; s++)
            {
                if (steps[s].set.inputs == null || steps[s].set.inputs.Length == 0)
                {
                    Debug.LogWarning($"{name}: step {s + 1} has no inputs.", this);
                }

                var hasEffects = steps[s].effects != null && steps[s].effects.Length > 0;
                if (steps[s].move == null && hasEffects)
                {
                    Debug.LogWarning($"{name}: step {s + 1} has effects but no move — they never resolve.", this);
                }
                else if (steps[s].move != null && !hasEffects)
                {
                    Debug.LogWarning($"{name}: step {s + 1} has a move but no effects — it never plays.", this);
                }
            }
        }
    }
}
