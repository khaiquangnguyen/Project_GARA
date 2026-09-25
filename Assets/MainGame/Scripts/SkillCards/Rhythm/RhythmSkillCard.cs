using GARA.Characters;
using GARA.Rhythm;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.SkillCards.Rhythm
{
    // A skill card whose input minigame is a rhythm sequence, played through
    // the host's RhythmSequencePlayer driver. Class-agnostic — any character
    // with a RhythmSequencePlayer on its input host can use one of these.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Rhythm Skill Card", fileName = "RhythmSkillCard")]
    public class RhythmSkillCard : SkillCardDefinition
    {
        [SerializeField]
        [Expandable]
        private RhythmSequenceDefinition sequence;

        [SerializeField]
        private RhythmScoreSource scoreSource = RhythmScoreSource.Accuracy;

        [SerializeField, Range(0, 1)]
        private float completionGate = 0.5f;

        [SerializeField, Range(0, 1)]
        private float strayPressPenalty = 0.02f;

        public RhythmSequenceDefinition Sequence => sequence;
        public RhythmScoreSource ScoreSource => scoreSource;
        public float CompletionGate => completionGate;
        public float StrayPressPenalty => strayPressPenalty;

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            return new RhythmSkillInputSession(host.GetDriver<RhythmSequencePlayer>(), this);
        }

        protected virtual void OnValidate()
        {
            if (sequence == null)
            {
                Debug.LogWarning($"{name}: RhythmSkillCard has no sequence assigned.", this);
            }
        }
    }
}
