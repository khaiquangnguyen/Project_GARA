using System;
using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters
{
    // Per-participant collection of live PassiveRuntimeStates, one per
    // PassiveDefinition the owner's CharacterDefinition authors. Built once
    // (see CombatParticipant.Passives) and lives for the whole battle.
    public sealed class PassiveRuntimeSet
    {
        private readonly Dictionary<PassiveDefinition, PassiveRuntimeState> _statesByDefinition = new Dictionary<PassiveDefinition, PassiveRuntimeState>();
        private readonly List<PassiveDefinition> _passives = new List<PassiveDefinition>();

        public PassiveRuntimeSet(IReadOnlyList<PassiveDefinition> passives)
        {
            if (passives == null)
            {
                return;
            }

            foreach (var passive in passives)
            {
                if (passive == null || _statesByDefinition.ContainsKey(passive))
                {
                    continue;
                }

                var state = passive.CreateRuntimeState();
                _statesByDefinition[passive] = state;
                _passives.Add(passive);
            }
        }

        public IReadOnlyList<PassiveDefinition> Passives => _passives;

        public PassiveRuntimeState GetState(PassiveDefinition passive)
        {
            return _statesByDefinition.TryGetValue(passive, out var state) ? state : null;
        }

        public TState GetState<TState>(PassiveDefinition passive) where TState : PassiveRuntimeState
        {
            return GetState(passive) as TState;
        }

        public void NotifySkillCardResolved(in PassiveContext context)
        {
            foreach (var passive in _passives)
            {
                var state = _statesByDefinition[passive];
                try
                {
                    passive.OnSkillCardResolved(in context, state);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public bool TryGetRangeRoller(out IValueRangeRoller roller)
        {
            foreach (var passive in _passives)
            {
                var candidate = passive.GetRangeRoller(_statesByDefinition[passive]);
                if (candidate != null)
                {
                    roller = candidate;
                    return true;
                }
            }

            roller = null;
            return false;
        }

        // Asks every passive (no short-circuit) so each clears its own pending
        // flag; multiple grants still collapse into one extra turn.
        public bool TryConsumeExtraTurn()
        {
            var granted = false;
            foreach (var passive in _passives)
            {
                granted |= passive.TryConsumeExtraTurn(_statesByDefinition[passive]);
            }

            return granted;
        }

        public void ResetAll()
        {
            foreach (var state in _statesByDefinition.Values)
            {
                state.Reset();
            }
        }
    }
}
