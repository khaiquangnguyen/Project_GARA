using UnityEngine;

namespace GARA.Characters
{
    // One authored effect a SkillCardDefinition applies once its input
    // minigame resolves — damage, a timed stat modifier, etc. A card can
    // list several; each resolves independently against the same context.
    public abstract class SkillEffectDefinition : ScriptableObject
    {
        public abstract void Resolve(in SkillEffectContext context);
    }
}
