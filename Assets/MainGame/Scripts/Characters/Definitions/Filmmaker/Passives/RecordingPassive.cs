using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GARA.Characters.Filmmaker
{
    // Filmmaker passive: each perfect parry of an opponent's skill adds a
    // stack to that skill (stacked per skill). At its parriesToRecord, the
    // skill is offered on the Filmmaker's turn: equipped (up to maxEquipped,
    // replacing one when full) as a copy played in its form, or skipped.
    // Either way its stacks count again from what's left over.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Recording", fileName = "RecordingPassive")]
    public class RecordingPassive : PassiveDefinition<RecordingState>
    {
        [Tooltip("Copied skills the Filmmaker can have equipped at once.")]
        [Min(1)]
        [SerializeField] private int maxEquipped = 2;

        [SerializeField] private FormReplayMode replayMode = FormReplayMode.StandIn;

        protected override void OnSkillPerfectlyParried(ICombatTarget self, ICombatTarget attacker, SkillCardDefinition card, RecordingState state)
        {
            // A copy is recorded as the skill it copies.
            var form = attacker?.Definition;
            if (card is FormReplaySkillCard replay)
            {
                card = replay.SourceCard;
                form = replay.Form;
            }

            if (card == null || form == null || form.FindSkillCardState(card) == null || state.IsEquipped(card))
            {
                return;
            }

            var recording = state.RecordingOf(card) ?? state.StartRecording(card);
            recording.form = form;
            recording.stacks++;
            Debug.Log($"[{nameof(RecordingPassive)}] recording {card.displayName} {recording.stacks}/{card.parriesToRecord}.");
        }

        protected override bool TryGetSkillCardOffer(ICombatTarget self, ICollection<SkillCardDefinition> offered, RecordingState state, out SkillCardOffer offer)
        {
            foreach (var recording in state.recordings)
            {
                if (recording.stacks >= recording.card.parriesToRecord && !offered.Contains(recording.card) && !state.IsEquipped(recording.card))
                {
                    offer = new SkillCardOffer(this, recording.card, state.equipped.ToArray(), state.equipped.Count < maxEquipped);
                    return true;
                }
            }

            offer = default;
            return false;
        }

        protected override void ResolveSkillCardOffer(ICombatTarget self, in SkillCardOffer offer, SkillCardOfferChoice choice, RecordingState state)
        {
            var recording = state.RecordingOf(offer.card);
            if (recording == null)
            {
                return;
            }

            // Extra stacks carry over.
            recording.stacks = Mathf.Max(0, recording.stacks - offer.card.parriesToRecord);
            if (!choice.accept)
            {
                Debug.Log($"[{nameof(RecordingPassive)}] skipped {offer.card.displayName}.");
                return;
            }

            if (choice.replaced is FormReplaySkillCard replaced && state.equipped.Remove(replaced))
            {
                self.RemoveSkillCard(replaced);
            }
            else if (state.equipped.Count >= maxEquipped)
            {
                return;
            }

            var copy = FormReplaySkillCard.Create(offer.card, recording.form, replayMode, oneTimeUse: false);
            if (copy == null)
            {
                return;
            }

            state.equipped.Add(copy);
            self.AddSkillCard(copy);
            Debug.Log($"[{nameof(RecordingPassive)}] equipped {copy.displayName}.");
        }

        protected override void OnSkillCardResolved(in PassiveContext context, RecordingState state)
        {
        }
    }
}
