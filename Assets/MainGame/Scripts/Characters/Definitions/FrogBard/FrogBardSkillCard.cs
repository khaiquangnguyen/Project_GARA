using GARA.Characters;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using UnityEngine;

namespace GARA.Characters.FrogBard
{
    // A song; every effect on it is a DualEmotionEffect, so it plays one way
    // in Joy and another in Sadness.
    [CreateAssetMenu(menuName = "GARA/Characters/Frog Bard/Song Card", fileName = "SongCard")]
    public class FrogBardSkillCard : RhythmSkillCard
    {
        protected override RhythmSequenceTiming TimingFor(CharacterDefinition actor)
        {
            return actor is FrogBard bard ? bard.SpecialTiming : RhythmSequenceTiming.Default;
        }

        protected override SkillPerformanceTiering TieringFor(CharacterDefinition actor)
        {
            return actor is FrogBard bard ? bard.SpecialTiering : SkillPerformanceTiering.Default;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            for (var b = 0; b < Bars.Count; b++)
            {
                WarnIfNotDual(Bars[b].effects, $"bar {b + 1}");
            }

            WarnIfNotDual(finaleEffects, "finale");
        }

        private void WarnIfNotDual(RhythmStepEffect[] effects, string where)
        {
            if (effects == null)
            {
                return;
            }

            foreach (var effect in effects)
            {
                if (effect != null && !(effect is DualEmotionEffect))
                {
                    Debug.LogWarning($"{name}: {where} has a {effect.GetType().Name} — Frog Bard effects should be DualEmotionEffect.", this);
                }
            }
        }
    }
}
