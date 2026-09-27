using System.Collections.Generic;

namespace GARA.Characters.Mimic
{
    // The skills copied at battle start, kept for the whole battle.
    public sealed class MimicryState : PassiveRuntimeState
    {
        public readonly List<FormReplaySkillCard> copies = new List<FormReplaySkillCard>();
    }
}
