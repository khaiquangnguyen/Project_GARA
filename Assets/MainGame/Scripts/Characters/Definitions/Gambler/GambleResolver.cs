using GARA.Characters;
using GARA.SkillCards.Shake;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Drives a GamblerSkillCard's game/rig once its shake-input manipulation
    // minigame has produced a SkillPerformance, and turns the resulting
    // GambleOutcome back into a generic SkillPerformance so the rest of the
    // skill-card pipeline (effects, tiering) doesn't need to know gambling
    // exists.
    public static class GambleResolver
    {
        public static SkillPerformanceTier ToTier(GambleResult result, bool consolation, SkillPerformanceTier manipulationTier)
        {
            switch (result)
            {
                case GambleResult.Jackpot:
                    return SkillPerformanceTier.Perfect;

                case GambleResult.Success:
                    return SkillPerformanceTier.Good;

                case GambleResult.Fail:
                    return consolation && manipulationTier == SkillPerformanceTier.Perfect
                        ? SkillPerformanceTier.Ok
                        : SkillPerformanceTier.Miss;

                case GambleResult.Bust:
                default:
                    return SkillPerformanceTier.Miss;
            }
        }

        public static SkillPerformance Resolve(GamblerSkillCard card, SkillPerformance manipulation, ShakeSkillReport shakeReport, IGambleRandom rng)
        {
            if (manipulation.WasAborted)
            {
                var abortedResolution = new GambleResolution(card.Game.GameType, manipulation, shakeReport, 0f, default, card.Rig);
                return SkillPerformance.Failed(abortedResolution);
            }

            var manipulate = card.ManipulateByTier.For(manipulation.Tier);

            var context = new GambleContext
            {
                Manipulate = manipulate,
                ConsolationOnFail = card.ConsolationOnFail
            };

            card.Rig?.BeforeResolve(context);

            var outcome = card.Game.Resolve(context, rng);

            card.Rig?.AfterResolve(context, ref outcome);

            var tier = ToTier(outcome.Result, context.ConsolationOnFail, manipulation.Tier);

            // Flat per-tier base (Miss/Ok/Good/Perfect = 0/.33/.66/1), nudged
            // by how decisive the roll's margin was. Kept deliberately
            // simple — margin only shifts the score within its own tier
            // band, it never pushes the score into a neighboring tier.
            var tierBase = TierBaseScore(tier);
            var score = Mathf.Clamp01(tierBase + outcome.Margin * 0.3f);

            var resolution = new GambleResolution(card.Game.GameType, manipulation, shakeReport, manipulate, outcome, card.Rig);

            return new SkillPerformance(score, tier, false, resolution);
        }

        private static float TierBaseScore(SkillPerformanceTier tier)
        {
            switch (tier)
            {
                case SkillPerformanceTier.Miss:
                    return 0f;

                case SkillPerformanceTier.Ok:
                    return 0.33f;

                case SkillPerformanceTier.Good:
                    return 0.66f;

                case SkillPerformanceTier.Perfect:
                    return 1f;

                default:
                    return 0f;
            }
        }
    }
}
