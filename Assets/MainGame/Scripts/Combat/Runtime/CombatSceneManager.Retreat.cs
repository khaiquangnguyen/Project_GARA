using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using UnityEngine;

namespace GARA.Combat
{
    // "Retreat if not targeted" (see retreatIfNotTargeted): while a
    // One*/Multi* skill card plays, every living member of its target pool
    // that's neither targeted nor the actor steps back to one of its side's
    // retreat slots, and walks back once the action is over. That's never
    // more than two per side — a side's third member is always a target or
    // the actor — so each side has exactly two slots. Slots are handed out
    // in formation order so retreating characters never cross paths, and
    // each slot's own sorting order is applied for as long as someone
    // stands there, so the scene controls how they overlap.
    public partial class CombatSceneManager
    {
        [Header("Retreat")]
        [Tooltip("Exactly 2 slots for the left (player) side — slot 0 goes to whichever retreating character stands nearest the front of the formation.")]
        [SerializeField] private RetreatSlot[] leftRetreatSlots = new RetreatSlot[2];

        [Tooltip("Exactly 2 slots for the right (enemy) side — slot 0 goes to whichever retreating character stands nearest the front of the formation.")]
        [SerializeField] private RetreatSlot[] rightRetreatSlots = new RetreatSlot[2];

        private readonly List<CombatParticipant> _retreated = new();

        private void RetreatUntargeted(CombatParticipant actor, SkillCardDefinition card, IReadOnlyList<ICombatTarget> targets)
        {
            if (!retreatIfNotTargeted || card.targetMode.IsAll())
            {
                return;
            }

            var untargeted = LivingPoolOf(actor, card.targetMode.GetPool())
                .Where(participant => participant != actor && !targets.Contains(participant))
                .ToList();

            RetreatSide(untargeted, _battle.playerParty, leftRetreatSlots);
            RetreatSide(untargeted, _battle.enemyParty, rightRetreatSlots);
        }

        private void RetreatSide(List<CombatParticipant> untargeted, BattleParty party, RetreatSlot[] slots)
        {
            var slotIndex = 0;
            foreach (var formationSlot in party.Slots)
            {
                var participant = formationSlot.occupant;
                if (participant == null || !untargeted.Contains(participant)
                    || !_executors.TryGetValue(participant, out var executor))
                {
                    continue;
                }

                if (slotIndex >= slots.Length || slots[slotIndex].anchor == null)
                {
                    Debug.LogWarning($"[{nameof(CombatSceneManager)}] No retreat slot {slotIndex} assigned for {participant.definition.displayName}'s side — they stay in formation.", this);
                    return;
                }

                var slot = slots[slotIndex++];
                SetSortingOrder(participant, slot.sortingOrder);
                executor.RetreatTo(slot.anchor.position);
                _retreated.Add(participant);
            }
        }

        // Walks everyone still retreated back into formation and waits until
        // they've all arrived. Anyone defeated while retreated stays where
        // they fell — their death reaction already takes them off screen.
        private IEnumerator ReturnRetreated()
        {
            var returning = _retreated.Where(participant => !participant.IsDefeated).ToList();
            foreach (var participant in returning)
            {
                _executors[participant].ReturnFromRetreat();
            }

            yield return new WaitUntil(() => returning.All(participant => participant.IsDefeated || !_executors[participant].IsBusy));

            foreach (var participant in returning)
            {
                SetSortingOrder(participant, 0);
            }

            _retreated.Clear();
        }

        // Safety rail for an aborted card: puts everyone retreated straight
        // back into formation rather than leaving them stranded.
        private void SnapRetreatedBack()
        {
            foreach (var participant in _retreated)
            {
                if (!participant.IsDefeated)
                {
                    _executors[participant].SnapToStandardPosition();
                }

                SetSortingOrder(participant, 0);
            }

            _retreated.Clear();
        }
    }
}
