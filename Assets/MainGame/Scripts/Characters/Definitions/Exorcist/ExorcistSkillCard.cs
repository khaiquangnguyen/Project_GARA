using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Tiering and sequence timing come from the Exorcist using the card; the
    // card supplies only its bars.
    [CreateAssetMenu(menuName = "GARA/Characters/Exorcist/Rite Card", fileName = "ExorcistRite")]
    public class ExorcistSkillCard : RhythmSkillCard
    {
        protected override RhythmSequenceTiming TimingFor(CharacterDefinition actor)
        {
            return actor is Exorcist exorcist ? exorcist.SpecialTiming : RhythmSequenceTiming.Default;
        }

        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is Exorcist exorcist ? exorcist.SpecialTiering : SkillPerformanceTiering.Default;
        }
    }
}
