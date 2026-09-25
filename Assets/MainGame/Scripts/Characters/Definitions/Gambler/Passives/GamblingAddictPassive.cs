using System;
using GARA.Characters;
using NaughtyAttributes;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Class passive: every resolved skill card grants chips (more on a
    // successful gamble), and once accumulated chips cross a threshold the
    // passive drains them into slot machine rolls, firing authored effects
    // on whichever outcome pattern matches.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Gambling Addict", fileName = "GamblingAddictPassive")]
    public class GamblingAddictPassive : PassiveDefinition<GamblingAddictState>
    {
        [Header("Chips")]
        [SerializeField]
        private int chipsPerCard = 1;

        [SerializeField]
        private int chipsOnSuccess = 3;

        [SerializeField]
        private int chipsOnFailure = 1;

        [SerializeField]
        private int chipThreshold = 10;

        [Header("Slot Machine")]
        [SerializeField]
        [Expandable]
        private SlotMachineDefinition slotMachine;

        [SerializeField]
        private SlotMachineOutcomeEffect[] outcomeEffects = Array.Empty<SlotMachineOutcomeEffect>();

        [SerializeField]
        private bool firstMatchOnly = true;

        [SerializeField]
        private int maxRollsPerTrigger = 8;

        protected override void OnSkillCardResolved(in PassiveContext context, GamblingAddictState state)
        {
            state.chips += chipsPerCard + (IsSuccess(context.Performance) ? chipsOnSuccess : chipsOnFailure);
            DrainChips(in context, state);
        }

        protected override IValueRangeRoller GetRangeRoller(GamblingAddictState state)
        {
            return state.Roller;
        }

        private static bool IsSuccess(in SkillPerformance performance)
        {
            if (performance.WasAborted)
            {
                return false;
            }

            if (performance.TryGetDetails<GambleResolution>(out var resolution))
            {
                return resolution.Outcome.Result == GambleResult.Success || resolution.Outcome.Result == GambleResult.Jackpot;
            }

            return performance.Tier == SkillPerformanceTier.Good || performance.Tier == SkillPerformanceTier.Perfect;
        }

        private void DrainChips(in PassiveContext context, GamblingAddictState state)
        {
            if (chipThreshold <= 0 || slotMachine == null)
            {
                return;
            }

            var rolls = 0;
            while (state.chips >= chipThreshold && rolls < maxRollsPerTrigger)
            {
                state.chips -= chipThreshold;
                state.totalRolls++;
                rolls++;
                PerformRoll(in context, state);
            }
        }

        private void PerformRoll(in PassiveContext context, GamblingAddictState state)
        {
            var roll = slotMachine.Roll(state.Rng);
            var performance = new SkillPerformance(1f, SkillPerformanceTier.Perfect, false, roll);

            foreach (var entry in outcomeEffects)
            {
                if (entry == null || !entry.pattern.Matches(roll, slotMachine))
                {
                    continue;
                }

                PassiveEffectRunner.Resolve(entry.effects, in context, entry.targetMode, performance);
                if (firstMatchOnly)
                {
                    break;
                }
            }
        }

        protected virtual void OnValidate()
        {
            if (chipsOnFailure <= 0)
            {
                Debug.LogWarning($"{name}: chipsOnFailure is <= 0 — a failed special will grant no chips at all.", this);
            }

            if (chipThreshold <= 0)
            {
                Debug.LogWarning($"{name}: chipThreshold must be positive.", this);
            }

            if (slotMachine != null && outcomeEffects != null)
            {
                foreach (var entry in outcomeEffects)
                {
                    if (entry.pattern.kind == SlotMatchKind.ExactCombination && entry.pattern.exactSymbolIds != null && entry.pattern.exactSymbolIds.Length != slotMachine.ReelCount)
                    {
                        Debug.LogWarning($"{name}: an ExactCombination outcome's symbol count doesn't match the slot machine's reel count.", this);
                    }
                }
            }
        }
    }
}
