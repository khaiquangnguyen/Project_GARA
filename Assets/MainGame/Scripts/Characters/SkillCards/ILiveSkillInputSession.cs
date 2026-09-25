using System;

namespace GARA.Characters
{
    // A session that plays its card's action as it goes: each step is played
    // and resolved while the minigame is still running.
    public interface ILiveSkillInputSession : ISkillInputSession
    {
        event Action<SkillStep> StepPerformed;

        // The first step's move, whose range the pre-input dash stands at.
        // Null when there's none to aim for.
        AttackAnimationSpec OpeningMove { get; }
    }
}
