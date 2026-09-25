using System;
using GARA.Characters;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // A skill card whose input minigame is a rhythm sequence, played through
    // the host's RhythmSequencePlayer driver. The card authors only its bars
    // (notes, plus the move each plays when cleared); lead-in, tail-out and
    // judging windows come from the character using it (TimingFor), and each
    // use builds a fresh runtime sequence. A card with any bar move is live.
    // The finale is the card's animationSpec.
    public abstract class RhythmSkillCard : SkillCardDefinition
    {
        // Drawn by RhythmSkillCardEditor as a flat timeline instead.
        [HideInInspector]
        [SerializeField]
        private RhythmCardBar[] bars = Array.Empty<RhythmCardBar>();

        [SerializeField]
        private RhythmScoreSource scoreSource = RhythmScoreSource.Accuracy;

        [SerializeField, Range(0, 1)]
        private float completionGate = 0.5f;

        [SerializeField, Range(0, 1)]
        private float strayPressPenalty = 0.02f;

        [NonSerialized]
        private RhythmSequenceDefinition _sequence;

        // The sequence of the current (or last) use.
        public RhythmSequenceDefinition Sequence => _sequence;
        public RhythmScoreSource ScoreSource => scoreSource;
        public float CompletionGate => completionGate;
        public float StrayPressPenalty => strayPressPenalty;

        public bool HasMoves
        {
            get
            {
                foreach (var bar in bars)
                {
                    if (bar.move != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool TryGetMove(int barIndex, out AttackAnimationSpec move)
        {
            move = barIndex >= 0 && barIndex < bars.Length ? bars[barIndex].move : null;
            return move != null;
        }

        protected abstract RhythmSequenceTiming TimingFor(CharacterDefinition actor);

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            if (_sequence != null)
            {
                Destroy(_sequence);
            }

            var sequenceBars = Array.ConvertAll(bars, bar => new RhythmBar { notes = bar.notes ?? Array.Empty<RhythmNote>() });
            _sequence = RhythmSequenceDefinition.CreateRuntime(TimingFor(host.Actor), sequenceBars);
            var player = host.GetDriver<RhythmSequencePlayer>();
            return HasMoves
                ? new RhythmLiveSkillInputSession(player, this)
                : new RhythmSkillInputSession(player, this);
        }

        protected virtual void OnValidate()
        {
            if (bars.Length == 0)
            {
                Debug.LogWarning($"{name}: RhythmSkillCard has no bars.", this);
            }
        }
    }
}
