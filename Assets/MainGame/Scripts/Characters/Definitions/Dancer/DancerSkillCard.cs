using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Dancer
{
    // Tiering and sequence timing (lead-in, tail-out, windows) come from the
    // Dancer using the card; the card supplies only its bars.
    [CreateAssetMenu(menuName = "GARA/Characters/Dancer/Dance Card", fileName = "DanceCard")]
    public class DancerSkillCard : RhythmSkillCard
    {
        protected override RhythmSequenceTiming TimingFor(CharacterDefinition actor)
        {
            return actor is Dancer dancer ? dancer.SpecialTiming : RhythmSequenceTiming.Default;
        }

        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is Dancer dancer ? dancer.SpecialTiering : SkillPerformanceTiering.Default;
        }
    }
}
