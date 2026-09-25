using GARA.Shake;

namespace GARA.SkillCards.Shake
{
    // Wraps the raw ShakeInputReport with the card-specific interpretation
    // (target pairs, overflow, progress). Handed back as SkillPerformance's
    // Details payload so a bespoke effect can read it via TryGetDetails.
    //
    // A sealed class rather than a struct: SkillPerformance.TryGetDetails<T>
    // is constrained to `where T : class`, so any Details payload that needs
    // to round-trip through it must be a reference type.
    public sealed class ShakeSkillReport
    {
        public readonly ShakeInputReport Raw;
        public readonly int TargetPairs;
        public readonly int OverflowPairs;
        public readonly float Progress;
        public readonly bool ReachedTarget;

        public ShakeSkillReport(ShakeInputReport raw, int targetPairs, int overflowPairs, float progress, bool reachedTarget)
        {
            Raw = raw;
            TargetPairs = targetPairs;
            OverflowPairs = overflowPairs;
            Progress = progress;
            ReachedTarget = reachedTarget;
        }
    }
}
