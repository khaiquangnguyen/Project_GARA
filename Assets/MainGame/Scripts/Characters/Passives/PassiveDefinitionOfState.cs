using System.Collections.Generic;

namespace GARA.Characters
{
    // Generic convenience base for PassiveDefinition — handles the runtime
    // state creation/casting boilerplate so concrete passives just implement
    // the typed overloads below. Named without the <T> since a filename
    // can't carry it.
    public abstract class PassiveDefinition<TState> : PassiveDefinition
        where TState : PassiveRuntimeState, new()
    {
        public sealed override PassiveRuntimeState CreateRuntimeState()
        {
            var state = new TState();
            state.Definition = this;
            return state;
        }

        public sealed override void OnSkillCardResolved(in PassiveContext context, PassiveRuntimeState state)
        {
            if (state is TState typed)
            {
                OnSkillCardResolved(in context, typed);
            }
        }

        protected abstract void OnSkillCardResolved(in PassiveContext context, TState state);

        public sealed override void OnParticipantDefeated(in PassiveContext context, ICombatTarget defeated, PassiveRuntimeState state)
        {
            if (state is TState typed)
            {
                OnParticipantDefeated(in context, defeated, typed);
            }
        }

        protected virtual void OnParticipantDefeated(in PassiveContext context, ICombatTarget defeated, TState state)
        {
        }

        public sealed override void OnHitLanded(ICombatTarget self, ICombatTarget target, int damage, PassiveRuntimeState state)
        {
            if (state is TState typed)
            {
                OnHitLanded(self, target, damage, typed);
            }
        }

        protected virtual void OnHitLanded(ICombatTarget self, ICombatTarget target, int damage, TState state)
        {
        }

        public sealed override void OnSkillPerfectlyParried(ICombatTarget self, ICombatTarget attacker, SkillCardDefinition card, PassiveRuntimeState state)
        {
            if (state is TState typed)
            {
                OnSkillPerfectlyParried(self, attacker, card, typed);
            }
        }

        protected virtual void OnSkillPerfectlyParried(ICombatTarget self, ICombatTarget attacker, SkillCardDefinition card, TState state)
        {
        }

        public sealed override bool TryGetSkillCardOffer(ICombatTarget self, ICollection<SkillCardDefinition> offered, PassiveRuntimeState state, out SkillCardOffer offer)
        {
            if (state is TState typed)
            {
                return TryGetSkillCardOffer(self, offered, typed, out offer);
            }

            offer = default;
            return false;
        }

        protected virtual bool TryGetSkillCardOffer(ICombatTarget self, ICollection<SkillCardDefinition> offered, TState state, out SkillCardOffer offer)
        {
            offer = default;
            return false;
        }

        public sealed override void ResolveSkillCardOffer(ICombatTarget self, in SkillCardOffer offer, SkillCardOfferChoice choice, PassiveRuntimeState state)
        {
            if (state is TState typed)
            {
                ResolveSkillCardOffer(self, in offer, choice, typed);
            }
        }

        protected virtual void ResolveSkillCardOffer(ICombatTarget self, in SkillCardOffer offer, SkillCardOfferChoice choice, TState state)
        {
        }

        public sealed override void OnBattleStarted(in PassiveContext context, PassiveRuntimeState state)
        {
            if (state is TState typed)
            {
                OnBattleStarted(in context, typed);
            }
        }

        protected virtual void OnBattleStarted(in PassiveContext context, TState state)
        {
        }

        public sealed override IValueRangeRoller GetRangeRoller(PassiveRuntimeState state)
        {
            return state is TState typed ? GetRangeRoller(typed) : null;
        }

        protected virtual IValueRangeRoller GetRangeRoller(TState state) => null;

        public sealed override bool TryConsumeExtraTurn(PassiveRuntimeState state)
        {
            return state is TState typed && TryConsumeExtraTurn(typed);
        }

        protected virtual bool TryConsumeExtraTurn(TState state) => false;
    }
}
