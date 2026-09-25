using UnityEngine;

namespace GARA.Characters
{
    // Authored asset for one always-on class passive. Stateless by design —
    // any mutable state lives in the PassiveRuntimeState this creates, kept
    // on the owning participant's PassiveRuntimeSet, so the same asset can
    // be shared across every character with the passive.
    public abstract class PassiveDefinition : ScriptableObject
    {
        public string passiveId;

        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;

        public abstract PassiveRuntimeState CreateRuntimeState();

        public abstract void OnSkillCardResolved(in PassiveContext context, PassiveRuntimeState state);

        public virtual IValueRangeRoller GetRangeRoller(PassiveRuntimeState state) => null;
    }
}
