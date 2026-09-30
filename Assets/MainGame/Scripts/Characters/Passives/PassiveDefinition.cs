using System.Collections.Generic;
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

        // Any participant on either side (never the owner itself) was just
        // defeated. context.Card is null.
        public virtual void OnParticipantDefeated(in PassiveContext context, ICombatTarget defeated, PassiveRuntimeState state)
        {
        }

        // The owner's skill just landed damage on target (not a parried,
        // evaded or blocked hit). Fires once per hit, mid-effect.
        public virtual void OnHitLanded(ICombatTarget self, ICombatTarget target, int damage, PassiveRuntimeState state)
        {
        }

        // The owner parried every hit attacker's card aimed at them.
        public virtual void OnSkillPerfectlyParried(ICombatTarget self, ICombatTarget attacker, SkillCardDefinition card, PassiveRuntimeState state)
        {
        }

        // The next card to offer the owner as their turn starts, skipping
        // those already offered this turn (see SkillCardOffer).
        public virtual bool TryGetSkillCardOffer(ICombatTarget self, ICollection<SkillCardDefinition> offered, PassiveRuntimeState state, out SkillCardOffer offer)
        {
            offer = default;
            return false;
        }

        public virtual void ResolveSkillCardOffer(ICombatTarget self, in SkillCardOffer offer, SkillCardOfferChoice choice, PassiveRuntimeState state)
        {
        }

        // Once, before the battle's first turn. context.Card is null.
        public virtual void OnBattleStarted(in PassiveContext context, PassiveRuntimeState state)
        {
        }

        public virtual IValueRangeRoller GetRangeRoller(PassiveRuntimeState state) => null;

        // Asked once as the owner's turn ends; true grants them another turn.
        public virtual bool TryConsumeExtraTurn(PassiveRuntimeState state) => false;
    }
}
