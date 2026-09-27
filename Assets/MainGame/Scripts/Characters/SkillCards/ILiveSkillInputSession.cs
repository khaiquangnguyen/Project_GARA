using System;

namespace GARA.Characters
{
    // A session that plays its card's action as it goes: each step is played
    // and resolved while the minigame is still running.
    public interface ILiveSkillInputSession : ISkillInputSession
    {
        event Action<SkillStep> StepPerformed;

        // A step's input is starting: its index and the move it will play
        // (null if none), so the performer can get into position early.
        event Action<int, AttackAnimationSpec> StepStarting;

        // The first step's move, whose range the pre-input dash stands at.
        // Null when there's none to aim for.
        AttackAnimationSpec OpeningMove { get; }
    }
}
