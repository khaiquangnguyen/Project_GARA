using GARA.Characters;
using GARA.InputSets;
using GARA.SkillCards.InputSets;
using UnityEngine;

namespace GARA.Characters.Bard
{
    // A song: each step is a stratagem code (an ordered arrow input set).
    // Its effects are DualEmotionEffects, playing one way in Joy and another
    // in Sadness, plus an optional MoodChangeEffect.
    [CreateAssetMenu(menuName = "GARA/Characters/Bard/Song Card", fileName = "SongCard")]
    public class BardSkillCard : LiveInputSetSkillCard
    {
        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is Bard bard ? bard.SpecialTiering : SkillPerformanceTiering.Default;
        }

        protected override InputSetScoreModel ScoreModelFor(CharacterDefinition actor)
        {
            return actor is Bard bard ? bard.SpecialScoreModel : InputSetScoreModel.Default;
        }

        protected override InputSetRetryPolicy RetryPolicyFor(CharacterDefinition actor)
        {
            return actor is Bard bard ? bard.SpecialRetryPolicy : InputSetRetryPolicy.Default;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            for (var s = 0; s < Steps.Count; s++)
            {
                WarnIfNotBardEffect(Steps[s].effects, $"step {s + 1}");
            }

            WarnIfNotBardEffect(finaleEffects, "finale");
        }

        private void WarnIfNotBardEffect(InputSetStepEffect[] effects, string where)
        {
            if (effects == null)
            {
                return;
            }

            foreach (var effect in effects)
            {
                if (effect != null && !(effect is DualEmotionEffect) && !(effect is MoodChangeEffect))
                {
                    Debug.LogWarning($"{name}: {where} has a {effect.GetType().Name} — Bard effects should be DualEmotionEffect or MoodChangeEffect.", this);
                }
            }
        }
    }
}
