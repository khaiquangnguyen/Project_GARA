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
