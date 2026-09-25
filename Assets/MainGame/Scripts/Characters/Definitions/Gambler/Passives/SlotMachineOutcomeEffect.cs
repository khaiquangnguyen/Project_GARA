using System;
using GARA.Characters;
using NaughtyAttributes;

namespace GARA.Characters.Gambler
{
    // One authored slot machine result the Gambling Addict passive checks
    // for, and the effects/target mode it fires when the roll matches.
    [Serializable]
    public class SlotMachineOutcomeEffect
    {
        public string label;
        public SlotMatchPattern pattern;
        public SpecialTargetMode targetMode = SpecialTargetMode.AllEnemy;
        [Expandable]
        public SkillEffectDefinition[] effects = Array.Empty<SkillEffectDefinition>();
    }
}
