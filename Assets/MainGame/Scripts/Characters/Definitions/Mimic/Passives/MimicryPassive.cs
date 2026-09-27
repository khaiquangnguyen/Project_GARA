using System.Collections.Generic;
using UnityEngine;

namespace GARA.Characters.Mimic
{
    // Mimic passive: before the battle starts, it copies every skill its
    // opponents have. The copies are its specials for the whole battle,
    // each played in the form of an opponent that has it
    // (FormReplayMode.AssumeForm), even after that opponent falls.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Mimicry", fileName = "MimicryPassive")]
    public class MimicryPassive : PassiveDefinition<MimicryState>
    {
        [Tooltip("Most skills copied; the hand shows up to 6.")]
        [Min(1)]
        [SerializeField] private int maxCopies = 6;

        protected override void OnBattleStarted(in PassiveContext context, MimicryState state)
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

        protected override void OnSkillCardResolved(in PassiveContext context, MimicryState state)
        {
        }
    }
}
