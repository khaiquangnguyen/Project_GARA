using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.Rhythm;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // A skill card whose input minigame is a rhythm sequence, played through
    // the host's RhythmSequencePlayer driver. The card authors its bars
    // (notes, plus the move and effects each plays when cleared); lead-in,
    // tail-out, judging windows and tiering come from the character using
    // it (TimingFor, TieringFor). Always live; the finale is the card's
    // animationSpec and finaleEffects, gated on the whole run.
    public abstract class RhythmSkillCard : SkillCardDefinition
    {
        // Drawn by RhythmSkillCardEditor as a flat timeline instead.
        [HideInInspector]
        [SerializeField]
        private RhythmCardBar[] bars = Array.Empty<RhythmCardBar>();

        [Tooltip("Gated on the whole run; the triggered ones apply on the finale's hit. None triggered = no finale.")]
        [BoxGroup(FinaleGroup)]
        [SerializeReference]
        [SubclassPicker]
        public RhythmStepEffect[] finaleEffects = Array.Empty<RhythmStepEffect>();

        [NonSerialized]
        private RhythmSequenceDefinition _sequence;

        // The sequence of the current (or last) use.
        public RhythmSequenceDefinition Sequence => _sequence;
        public IReadOnlyList<RhythmCardBar> Bars => bars;

        protected override bool IsLive => true;

        protected override bool HasCardEffects => false;

        protected override bool HasPerfectEffects => false;

        // The move of the earliest-played bar that has one.
        public AttackAnimationSpec OpeningMove
        {
            get
            {
                AttackAnimationSpec opening = null;
                var openingTime = float.MaxValue;
                foreach (var bar in bars)
                {
                    if (bar.move == null || bar.notes == null)
                    {
                        continue;
                    }

                    foreach (var note in bar.notes)
                    {
                        if (note.time < openingTime)
                        {
                            openingTime = note.time;
                            opening = bar.move;
                        }
                    }
                }

                return opening;
            }
        }

        protected abstract RhythmSequenceTiming TimingFor(CharacterDefinition actor);

        protected abstract SkillPerformanceTiering TieringFor(CharacterDefinition actor);

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            if (_sequence != null)
            {
                Destroy(_sequence);
            }

            var sequenceBars = Array.ConvertAll(bars, bar => new RhythmBar { notes = bar.notes ?? Array.Empty<RhythmNote>() });
            _sequence = RhythmSequenceDefinition.CreateRuntime(TimingFor(host.Actor), sequenceBars);
            var player = host.GetDriver<RhythmSequencePlayer>();
            return new RhythmLiveSkillInputSession(player, this, TieringFor(host.Actor));
        }

        protected virtual void OnValidate()
        {
            if (bars.Length == 0)
            {
                Debug.LogWarning($"{name}: RhythmSkillCard has no bars.", this);
            }

            for (var b = 0; b < bars.Length; b++)
            {
                var hasEffects = bars[b].effects != null && bars[b].effects.Length > 0;
                if (bars[b].move == null && hasEffects)
                {
                    Debug.LogWarning($"{name}: bar {b + 1} has effects but no move — they never resolve.", this);
                }
                else if (bars[b].move != null && !hasEffects)
                {
                    Debug.LogWarning($"{name}: bar {b + 1} has a move but no effects — it never plays.", this);
                }
            }
        }
    }
}
