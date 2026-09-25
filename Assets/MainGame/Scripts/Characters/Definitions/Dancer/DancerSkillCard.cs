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
    public class DancerSkillCard : RhythmSkillCard
    {
        [NonSerialized]
        private Dancer _owner;

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
