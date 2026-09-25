using System;

namespace GARA.Characters
{
    // One run of a skill card's input minigame. Begin starts it; onCompleted
    // must fire exactly once per Begin call — including when Abort() cuts
    // the session short, in which case it should fire with a failed/aborted
    // SkillPerformance rather than not firing at all.
    public interface ISkillInputSession
    {
        void Begin(Action<SkillPerformance> onCompleted);

        void Abort();
    }
}
