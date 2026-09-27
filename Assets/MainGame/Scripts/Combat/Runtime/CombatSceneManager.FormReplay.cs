using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using Spine.Unity;
using UnityEngine;

namespace GARA.Combat
{
    // Who performs the current card. Normally the actor; for a
    // FormReplaySkillCard, a stand-in built from the card's form performs
    // the source card's own state while the actor falls back (StandIn) or
    // stays hidden in its place (AssumeForm). Either way the actor owns the
    // card: its effects, passives and costs are the actor's.
    public partial class CombatSceneManager
    {
        private CombatParticipant _performer;
        private AttackExecutor _performerExecutor;

        private FormReplaySkillCard _replayCard;
        private GameObject _standInRoot;
        private readonly List<Renderer> _hiddenActorRenderers = new();

        private void BeginPerformance(SkillCardDefinition card)
        {
            _performer = _actor;
            _performerExecutor = _actorExecutor;

            if (!(card is FormReplaySkillCard replay) || replay.Form == null)
            {
                return;
            }

            var actorTransform = _actor.SceneTransform;
            var instance = Instantiate(replay.Form.gameObject, combatCharactersParent);
            instance.transform.SetPositionAndRotation(actorTransform.position, actorTransform.rotation);
            if (!instance.TryGetComponent<AttackExecutor>(out var executor))
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] {replay.Form.name} has no {nameof(AttackExecutor)} — {_actor.definition.displayName} plays {replay.name} itself.", replay.Form);
                Destroy(instance);
                return;
            }

            // Mirrored when the form was made to face the other side.
            if (replay.Form.faction != _actor.faction)
            {
                foreach (var skeleton in instance.GetComponentsInChildren<SkeletonRenderer>(true))
                {
                    skeleton.initialFlipX = !skeleton.initialFlipX;
                    skeleton.Initialize(true);
                }
            }

            var standIn = CombatParticipant.CreateStandIn(replay.Form, _actor.Allegiance);
            standIn.BindToSceneInstance(instance);
            executor.Initialize(standIn, _battle.QueryFor(_actor));

            _replayCard = replay;
            _standInRoot = instance;
            _performer = standIn;
            _performerExecutor = executor;
            SetSortingOrder(standIn, 1);

            if (replay.Mode == FormReplayMode.AssumeForm)
            {
                HideActor();
            }
            else
            {
                FallBack(_actor);
            }
        }

        // The performer's state for card: a replay plays its source card's.
        private CharacterState PerformerStateOf(SkillCardDefinition card)
        {
            return _performer.SkillCardStateOf(card is FormReplaySkillCard replay ? replay.SourceCard : card);
        }

        // Takes the next free retreat slot on the actor's side; returns with
        // everyone else (ReturnRetreated / SnapRetreatedBack).
        private void FallBack(CombatParticipant actor)
        {
            var slots = actor.faction == FactionTag.Player ? leftRetreatSlots : rightRetreatSlots;
            var slotIndex = _retreated.Count(participant => participant.faction == actor.faction);
            if (slotIndex >= slots.Length || slots[slotIndex].anchor == null)
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] No retreat slot {slotIndex} for {actor.definition.displayName} to fall back to — it stays put.", this);
                return;
            }

            var slot = slots[slotIndex];
            SetSortingOrder(actor, slot.sortingOrder);
            _actorExecutor.RetreatTo(slot.anchor.position);
            _retreated.Add(actor);
        }

        private void HideActor()
        {
            foreach (var actorRenderer in _actor.SceneRoot.GetComponentsInChildren<Renderer>())
            {
                if (actorRenderer.enabled)
                {
                    actorRenderer.enabled = false;
                    _hiddenActorRenderers.Add(actorRenderer);
                }
            }
        }

        // Removes the stand-in and shows the actor again. Safe to call when
        // no replay is playing.
        private void EndPerformance()
        {
            if (_standInRoot != null)
            {
                Destroy(_standInRoot);
            }

            foreach (var actorRenderer in _hiddenActorRenderers)
            {
                if (actorRenderer != null)
                {
                    actorRenderer.enabled = true;
                }
            }

            // A spent one-time copy; a reusable one stays in hand.
            if (_replayCard != null && _replayCard.IsOneTimeUse)
            {
                Destroy(_replayCard);
            }

            _hiddenActorRenderers.Clear();
            _standInRoot = null;
            _replayCard = null;
            _performer = _actor;
            _performerExecutor = _actorExecutor;
        }

        // Redraws the hand after it changed mid-phase (a one-time card
        // played, or a card granted by an effect).
        private void RefreshHand()
        {
            if (_actor == null)
            {
                return;
            }

            RefreshSkillCardSlots(_actor);
            if (_actor.IsPlayerControlled && !SetCardHighlight(_highlightedCardIndex))
            {
                ResetCardHighlight();
            }
        }
    }
}
