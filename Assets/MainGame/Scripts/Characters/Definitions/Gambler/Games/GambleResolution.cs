using GARA.Characters;
using GARA.SkillCards.Shake;

namespace GARA.Characters.Gambler
{
    // Full record of one gamble: the shake minigame that produced the
    // manipulate value, the game that was rolled, and the (possibly
    // rig-reshaped) outcome. Carried as GamblerSkillCard's SkillPerformance
    // Details payload so gamble-aware effects can read it back out.
    public sealed class GambleResolution
    {
        public readonly GamblingGameType GameType;
        public readonly SkillPerformance ManipulationPerformance;
        public readonly ShakeSkillReport ShakeReport;
        public readonly float ManipulatePercent;
        public readonly GambleOutcome Outcome;
        public readonly GambleRigDefinition Rig;

        public GambleResolution(
            GamblingGameType gameType,
            SkillPerformance manipulationPerformance,
            ShakeSkillReport shakeReport,
            float manipulatePercent,
            GambleOutcome outcome,
            GambleRigDefinition rig)
        {
            GameType = gameType;
            ManipulationPerformance = manipulationPerformance;
            ShakeReport = shakeReport;
            ManipulatePercent = manipulatePercent;
            Outcome = outcome;
            Rig = rig;
        }
    }
}
