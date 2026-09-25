namespace GARA.Characters
{
    // Outcome of one resolved skill-card input minigame. Score/Tier drive the
    // generic scaling (StatScaling, TierScaling); Details is an
    // input-system-specific payload (e.g. rhythm hit timings) that skill
    // effects generally ignore but a bespoke effect can read back out via
    // TryGetDetails.
    public readonly struct SkillPerformance
    {
        public readonly float Score;
        public readonly SkillPerformanceTier Tier;
        public readonly bool WasAborted;
        public readonly object Details;

        public SkillPerformance(float score, SkillPerformanceTier tier, bool wasAborted, object details)
        {
            Score = score;
            Tier = tier;
            WasAborted = wasAborted;
            Details = details;
        }

        public bool TryGetDetails<T>(out T details) where T : class
        {
            if (Details is T typed)
            {
                details = typed;
                return true;
            }

            details = null;
            return false;
        }

        public static SkillPerformance Failed(object details = null)
        {
            return new SkillPerformance(0f, SkillPerformanceTier.Miss, true, details);
        }
    }
}
