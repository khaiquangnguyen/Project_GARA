using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using GARA.Input;
using GARA.InputSets;
using GARA.Rhythm;
using UnityEngine;

namespace GARA.Combat
{
    // Dev tool hook (Character Test window): plays any card for any
    // character right now, skipping turn order, costs, loadout and side
    // checks, with targets picked from the card's usual pool. The card
    // borrows the actor slot; the turn's own actor is handed back once it
    // resolves.
    public partial class CombatSceneManager
    {
        private bool _debugPlaying;
        private CombatParticipant _debugSavedActor;
        private AttackExecutor _debugSavedExecutor;

        public IReadOnlyList<CombatParticipant> DebugParticipants =>
            _battle != null ? _battle.AllParticipants.ToList() : new List<CombatParticipant>();

        // Why a card can't be test-played right now, or null when it can.
        public string DebugPlayBlocker
        {
            get
            {
                if (_battle == null)
                {
                    return "No battle running.";
                }

                if (_phaseActionState == PhaseActionState.SkillCardInput || _phaseActionState == PhaseActionState.SkillCardResolving || _debugPlaying)
                {
                    return "A card is still playing.";
                }

                if (_aiActionInProgress || (_actor != null && !_actor.IsPlayerControlled))
                {
                    return "Wait for a player-controlled turn (an AI turn would start its own card).";
                }

                return null;
            }
        }

        public bool DebugPlaySkillCard(CombatParticipant performer, SkillCardDefinition card, AutoPlayMode mode, out string error)
        {
            error = DebugPlayBlocker;
            if (error != null)
            {
                return false;
            }

            if (performer == null || card == null || !_executors.TryGetValue(performer, out var executor))
            {
                error = "Pick a character and a card.";
                return false;
            }

            if (performer.IsDefeated)
            {
                error = $"{performer.definition.displayName} is defeated.";
                return false;
            }

            if (performer.definition.FindSkillCardState(card) == null && !(card is FormReplaySkillCard))
            {
                error = $"{performer.definition.displayName} has no state that plays {card.name}.";
                return false;
            }

            var pool = LivingPoolOf(performer, card.targetMode.GetPool());
            var targets = card.targetMode.IsSingle()
                ? pool.OrderBy(_ => Random.value).Take(1).ToList()
                : card.targetMode.PickRandomTargets(pool, card.multiTargetCount);
            if (targets.Count == 0)
            {
                error = $"No living targets for {card.displayName} ({card.targetMode}).";
                return false;
            }

            // Drop any half-picked card the player had open.
            if (_phaseActionState == PhaseActionState.SkillCardTargeting)
            {
                ClearTargetPicking();
                _phaseActionState = PhaseActionState.Regular;
            }

            _debugPlaying = true;
            _debugSavedActor = _actor;
            _debugSavedExecutor = _actorExecutor;
            _actor = performer;
            _actorExecutor = executor;

            var rhythm = skillCardInputHost.GetDriver<RhythmSequencePlayer>();
            if (rhythm != null)
            {
                rhythm.AutoPlay = mode;
            }

            var inputSets = skillCardInputHost.GetDriver<InputSetCollectionPlayer>();
            if (inputSets != null)
            {
                inputSets.AutoPlay = mode;
            }

            Debug.Log($"[{nameof(CombatSceneManager)}] Test-playing {card.displayName} as {performer.definition.displayName} ({mode}) on {string.Join(", ", targets.Select(t => t.definition.displayName))}.");
            StartSkillCard(card, targets.Cast<ICombatTarget>().ToList());
            return true;
        }

        // Hands the actor slot back to the turn's own actor after a test play.
        private void EndDebugPlay()
        {
            if (!_debugPlaying)
            {
                return;
            }

            _debugPlaying = false;
            var rhythm = skillCardInputHost.GetDriver<RhythmSequencePlayer>();
            if (rhythm != null)
            {
                rhythm.AutoPlay = AutoPlayMode.Off;
            }

            var inputSets = skillCardInputHost.GetDriver<InputSetCollectionPlayer>();
            if (inputSets != null)
            {
                inputSets.AutoPlay = AutoPlayMode.Off;
            }

            _actor = _debugSavedActor;
            _actorExecutor = _debugSavedExecutor;
            _debugSavedActor = null;
            _debugSavedExecutor = null;
            if (_actor != null)
            {
                AnnounceActiveActor(_actor, true);
                RefreshHand();
            }
        }
    }
}
