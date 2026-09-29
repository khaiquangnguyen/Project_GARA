using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Gambler passive: every card the Gambler finishes pulls the lever on the
    // slot machine arm. Each reel pays out its symbol's reward; all reels on
    // one symbol hit the jackpot and pay its jackpot reward instead.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Slot Machine", fileName = "SlotMachinePassive")]
    public class SlotMachinePassive : PassiveDefinition<SlotMachineState>
    {
        [Tooltip("Reels per spin; all of them on one symbol is a jackpot.")]
        [Min(2)]
        [SerializeField] private int reelCount = 3;

        [Tooltip("Whether an aborted card still pulls the lever.")]
        [SerializeField] private bool spinOnAbort;

        [SerializeField] private SlotSymbol[] symbols =
        {
            new SlotSymbol
            {
                symbolName = "Cherry",
                reward = new SelfHealSlotReward { amount = 8 },
                jackpotReward = new SelfHealSlotReward { amount = 40 }
            },
            new SlotSymbol
            {
                symbolName = "Bomb",
                reward = new DamageSlotReward { damage = 6 },
                jackpotReward = new DamageSlotReward { damage = 30 }
            },
            new SlotSymbol
            {
                symbolName = "Fist",
                reward = new PowerUpSlotReward { damageIncrease = 0.2f, turns = 2 },
                jackpotReward = new PowerUpSlotReward { damageIncrease = 1f, turns = 3 }
            },
            new SlotSymbol
            {
                symbolName = "Shield",
                reward = new SelfStatSlotReward { stat = StatKind.Defense, magnitude = 3, turns = 2 },
                jackpotReward = new SelfStatSlotReward { stat = StatKind.Defense, magnitude = 12, turns = 3 }
            }
        };

        private readonly IGambleRandom _rng = new UnityGambleRandom();

        protected override void OnSkillCardResolved(in PassiveContext context, SlotMachineState state)
        {
            if (context.Self == null || context.Self.IsDefeated || (context.Performance.WasAborted && !spinOnAbort))
            {
                return;
            }

            var spin = Spin();
            if (spin == null)
            {
                return;
            }

            var jackpot = IsJackpot(spin);
            state.Record(spin, jackpot);
            GambleEvents.RaiseSlotSpun(context.Self, spin, jackpot);
            Debug.Log($"[{nameof(SlotMachinePassive)}] {string.Join(" | ", Array.ConvertAll(spin, symbol => symbol.symbolName))}{(jackpot ? " — JACKPOT!" : "")}");

            if (jackpot)
            {
                spin[0].jackpotReward?.Apply(context);
                return;
            }

            foreach (var symbol in spin)
            {
                symbol.reward?.Apply(context);
            }
        }

        // Null when there's nothing to land on.
        private SlotSymbol[] Spin()
        {
            var totalWeight = 0;
            foreach (var symbol in symbols)
            {
                totalWeight += symbol != null ? symbol.weight : 0;
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            var spin = new SlotSymbol[reelCount];
            for (var reel = 0; reel < reelCount; reel++)
            {
                spin[reel] = Pick(_rng.Range(0, totalWeight));
            }

            return spin;
        }

        private SlotSymbol Pick(int roll)
        {
            foreach (var symbol in symbols)
            {
                if (symbol == null || symbol.weight <= 0)
                {
                    continue;
                }

                if (roll < symbol.weight)
                {
                    return symbol;
                }

                roll -= symbol.weight;
            }

            throw new InvalidOperationException("Roll exceeded the total symbol weight.");
        }

        private static bool IsJackpot(SlotSymbol[] spin)
        {
            for (var reel = 1; reel < spin.Length; reel++)
            {
                if (spin[reel] != spin[0])
                {
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            foreach (var symbol in symbols)
            {
                if (symbol != null && symbol.weight > 0 && (symbol.reward == null || symbol.jackpotReward == null))
                {
                    Debug.LogWarning($"{name}: slot symbol '{symbol.symbolName}' is missing its reward or jackpot reward.", this);
                }
            }
        }
    }
}
