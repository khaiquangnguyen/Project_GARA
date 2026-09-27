using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Filmmaker
{
    // A shot: each step is a stratagem code (an ordered arrow input set).
    [CreateAssetMenu(menuName = "GARA/Characters/Filmmaker/Shot Card", fileName = "ShotCard")]
    public class FilmmakerSkillCard : LiveInputSetSkillCard
    {
        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is Filmmaker filmmaker ? filmmaker.SpecialTiering : SkillPerformanceTiering.Default;
        }

        protected override InputSetScoreModel ScoreModelFor(CharacterDefinition actor)
        {
            return actor is Filmmaker filmmaker ? filmmaker.SpecialScoreModel : InputSetScoreModel.Default;
        }

        protected override InputSetRetryPolicy RetryPolicyFor(CharacterDefinition actor)
        {
            return actor is Filmmaker filmmaker ? filmmaker.SpecialRetryPolicy : InputSetRetryPolicy.Default;
        }
    }
}
