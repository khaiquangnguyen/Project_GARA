using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using MoreMountains.Tools;
using UnityEngine;

namespace GARA.Combat
{
    // "Retreat if not targeted" (see retreatIfNotTargeted): while an action
    // plays, every living character not involved in it (neither the actor
    // nor a target, on either side) steps back to one of its side's retreat
    // slots, and walks back once the action is over. A side can have all 3
    // members uninvolved, so each side has 3 slots, handed out in formation
    // order so retreating characters never cross paths. Each slot's own
    // sorting order applies while someone stands there.
    public partial class CombatSceneManager
    {
        [Header("Retreat")]
        [Tooltip("Exactly 3 slots for the left (player) side — slot 0 goes to whichever retreating character stands nearest the front of the formation.")]
        [SerializeField] private RetreatSlot[] leftRetreatSlots = new RetreatSlot[BattleParty.Size];

        [Tooltip("Exactly 3 slots for the right (enemy) side — slot 0 goes to whichever retreating character stands nearest the front of the formation.")]
        [SerializeField] private RetreatSlot[] rightRetreatSlots = new RetreatSlot[BattleParty.Size];

        private readonly List<CombatParticipant> _retreated = new();

        // Allies still in an alliesJoinFinale finale's dance window;
        // resolving waits for them.
        private int _finaleDancersPending;

        private void RetreatUninvolved(CombatParticipant actor, IReadOnlyList<ICombatTarget> targets)
        {
            if (!retreatIfNotTargeted)
            {
                return;
            }

            var uninvolved = LivingUninvolvedIn(actor, targets);
            RetreatSide(uninvolved, _battle.playerParty, leftRetreatSlots);
            RetreatSide(uninvolved, _battle.enemyParty, rightRetreatSlots);
        }

        private void RetreatSide(List<CombatParticipant> uninvolved, BattleParty party, RetreatSlot[] slots)
        {
            // After any slots this side already handed out.
            var slotIndex = _retreated.Count(participant => party.Slots.Any(slot => slot.occupant == participant));
            foreach (var formationSlot in party.Slots)
            {
                var participant = formationSlot.occupant;
                if (participant == null || !uninvolved.Contains(participant)
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

        // alliesJoinFinale: the actor's allies leave the stage for the card,
        // faded like anyone uninvolved even when they're its targets.
        private void RetreatAlliesForFinale(CombatParticipant actor, SkillCardDefinition card)
        {
            if (!card.alliesJoinFinale)
            {
                return;
            }

            var allies = LivingAlliesOf(actor).Where(ally => ally != actor && !_retreated.Contains(ally)).ToList();
            var onPlayerSide = actor.faction == FactionTag.Player;
            RetreatSide(allies, onPlayerSide ? _battle.playerParty : _battle.enemyParty, onPlayerSide ? leftRetreatSlots : rightRetreatSlots);
            foreach (var ally in allies)
            {
                MMEventManager.TriggerEvent(new TargetedStateEvent(ally.SceneRoot, false));
            }
        }

        // alliesJoinFinale: the allies walk back in, unfaded, and dance along
        // until allyDanceDuration (from now) runs out.
        private void BringAlliesBackToDance(CombatParticipant actor, float duration)
        {
            var allies = _retreated.Where(ally => ally != actor && ally.faction == actor.faction && !ally.IsDefeated).ToList();
            foreach (var ally in allies)
            {
                _retreated.Remove(ally);
                SetSortingOrder(ally, 0);
                MMEventManager.TriggerEvent(new TargetedStateEvent(ally.SceneRoot, true));
                var executor = _executors[ally];
                executor.ReturnFromRetreat();
                _finaleDancersPending++;
                StartCoroutine(DanceUntil(executor, Time.time + duration));
            }
        }

        // Random clips back to back once arrived, cut off at endTime.
        private IEnumerator DanceUntil(AttackExecutor executor, float endTime)
        {
            yield return new WaitUntil(() => !executor.IsBusy || Time.time >= endTime);
            while (Time.time < endTime && !executor.Participant.IsDefeated)
            {
                executor.PlayRandomEmote();
                if (!executor.IsBusy)
                {
                    break;
                }

                yield return new WaitUntil(() => !executor.IsBusy || Time.time >= endTime);
            }

            executor.StopEmote();
            _finaleDancersPending = Mathf.Max(0, _finaleDancersPending - 1);
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
