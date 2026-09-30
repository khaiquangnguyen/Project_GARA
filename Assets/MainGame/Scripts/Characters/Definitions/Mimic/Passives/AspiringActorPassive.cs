using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters.Mimic
{
    // Mimic passive: after every card but the first, it takes a stance.
    // The same card as last time = Method Acting (Defense up); a different
    // one = Range Acting (Attack up). One stance at a time, until the next
    // card. Also copies the opponents' skills at battle start as its hand.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Aspiring Actor", fileName = "AspiringActorPassive")]
    public class AspiringActorPassive : PassiveDefinition<AspiringActorState>
    {
        [Tooltip("Most skills copied; the hand shows up to 6.")]
        [Min(1)]
        [SerializeField] private int maxCopies = 6;

        [Header("Stances")]
        [Tooltip("Method Acting: fraction of base Defense added.")]
        [Min(0f)]
        [SerializeField] private float methodActingDefenseIncrease = 0.5f;

        [Tooltip("Range Acting: fraction of base Attack added.")]
        [Min(0f)]
        [SerializeField] private float rangeActingAttackIncrease = 0.5f;

        protected override void OnBattleStarted(in PassiveContext context, AspiringActorState state)
        {
            var seen = new HashSet<SkillCardDefinition>();
            foreach (var opponent in context.Battle.Enemies)
            {
                if (opponent.IsDefeated || opponent.Definition == null)
                {
                    continue;
                }

                foreach (var card in opponent.SkillCards)
                {
                    if (state.copies.Count >= maxCopies)
                    {
                        return;
                    }

                    // Never a copy of a copy.
                    if (card == null || card is FormReplaySkillCard || !seen.Add(card))
                    {
                        continue;
                    }

                    var copy = FormReplaySkillCard.Create(card, opponent.Definition, FormReplayMode.AssumeForm, oneTimeUse: false);
                    if (copy != null)
                    {
                        state.copies.Add(copy);
                        context.Self.AddSkillCard(copy);
                    }
                }
            }
        }

        protected override void OnSkillCardResolved(in PassiveContext context, AspiringActorState state)
        {
            var self = context.Self;
            if (context.Card == null || self == null || self.IsDefeated)
            {
                return;
            }

            // The first card of the battle leaves it unspecialized.
            var previous = state.lastCard;
            state.lastCard = context.Card;
            if (previous == null)
            {
                return;
            }

            self.TakeStatuses(IsStance);
            self.ApplyStatus(previous == context.Card ? CreateMethodActing() : CreateRangeActing());
        }

        private static bool IsStance(StatusEffectInstance status)
        {
            return status.kind is StatusEffectKind.MethodActing or StatusEffectKind.RangeActing;
        }

        private StatusEffectInstance CreateMethodActing()
        {
            var stance = StatusEffectInstance.Permanent(StatusEffectKind.MethodActing, passiveId);
            stance.defenseMultiplier = 1f + methodActingDefenseIncrease;
            return stance;
        }

        private StatusEffectInstance CreateRangeActing()
        {
            var stance = StatusEffectInstance.Permanent(StatusEffectKind.RangeActing, passiveId);
            stance.attackMultiplier = 1f + rangeActingAttackIncrease;
            return stance;
        }
    }
}
