using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // Shared helper for skill effects that want to react to how a
    // GamblerSkillCard's gamble resolved (its GambleResolution payload)
    // rather than just its tier/score.
    public abstract class GambleAwareSkillEffect : SkillEffectDefinition
    {
        protected static bool TryGetResolution(SkillPerformance performance, out GambleResolution resolution)
        {
            return performance.TryGetDetails(out resolution);
        }
    }
}
