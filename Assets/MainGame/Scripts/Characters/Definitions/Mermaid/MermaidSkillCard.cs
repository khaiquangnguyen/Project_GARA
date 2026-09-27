using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.Mermaid
{
    // Tiering and sequence timing come from the Mermaid using the card; the
    // card supplies only its bars.
    [CreateAssetMenu(menuName = "GARA/Characters/Mermaid/Song Card", fileName = "MermaidSong")]
    public class MermaidSkillCard : RhythmSkillCard
    {
        protected override RhythmSequenceTiming TimingFor(CharacterDefinition actor)
        {
            return actor is Mermaid mermaid ? mermaid.SpecialTiming : RhythmSequenceTiming.Default;
        }

        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is Mermaid mermaid ? mermaid.SpecialTiering : SkillPerformanceTiering.Default;
        }
    }
}
