using System.Collections.Generic;

namespace GARA.Characters.Mimic
{
    // The skills copied at battle start, and the last card played (which
    // decides the next stance).
    public sealed class AspiringActorState : PassiveRuntimeState
    {
        public readonly List<FormReplaySkillCard> copies = new List<FormReplaySkillCard>();

        public SkillCardDefinition lastCard;

        public override void Reset()
        {
            copies.Clear();
            lastCard = null;
        }
    }
}
