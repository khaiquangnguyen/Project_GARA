using System;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Tiering and sequence timing (lead-in, tail-out, windows) come from the
    // Dancer using the card (the input host's actor); the card supplies only
    // its bars.
    [CreateAssetMenu(menuName = "GARA/Characters/Dancer/Dance Card", fileName = "DanceCard")]
    public class DancerSkillCard : RhythmSkillCard, IPerfectAnnouncementCard
    {
        [Tooltip("This card's own drop prefab, dropped onto each target on the perfect finale and landing on its hit frame.")]
        [SerializeField]
        private GameObject perfectAnnouncementDrop;

        [NonSerialized]
        private Dancer _owner;

        public GameObject PerfectAnnouncementDrop => perfectAnnouncementDrop;

        protected override bool HasSharedTiering => true;

        protected override SkillPerformanceTiering Tiering => _owner != null ? _owner.SpecialTiering : base.Tiering;

        protected override RhythmSequenceTiming TimingFor(CharacterDefinition actor)
        {
            return actor is Dancer dancer ? dancer.SpecialTiming : RhythmSequenceTiming.Default;
        }

        public override ISkillInputSession CreateInputSession(ISkillInputHost host)
        {
            _owner = host.Actor as Dancer;
            return base.CreateInputSession(host);
        }
    }
}
