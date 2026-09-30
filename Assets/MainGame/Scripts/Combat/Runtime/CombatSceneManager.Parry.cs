using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // Parry input during an AI-controlled swing: opens a parry window on
    // every living player-controlled defender and plays their ParryState. A missed window is followed by a
    // cooldown; a successful parry allows an immediate retry. Timings live on
    // ParrySpec.
    public partial class CombatSceneManager
    {
        private InputAction _parry;
        private bool _aiActionInProgress;

        private float _parryCooldownEndsAt = float.NegativeInfinity;
        private bool _parryRegistered;

        // Per defender hit by the AI card in play: whether it parried every
        // hit so far.
        private readonly Dictionary<CombatParticipant, bool> _parriedEveryHit = new();

        private void EnableParry()
        {
            _parry.performed += OnParry;
            CombatParticipant.ParrySucceeded += OnParrySucceeded;
            CombatParticipant.HitNotParried += OnHitNotParried;
        }

        private void DisableParry()
        {
            _parry.performed -= OnParry;
            CombatParticipant.ParrySucceeded -= OnParrySucceeded;
            CombatParticipant.HitNotParried -= OnHitNotParried;
        }

        private void OnParry(InputAction.CallbackContext ctx)
        {
            if (!_aiActionInProgress)
            {
                return;
            }

            if (parrySpec == null)
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] No {nameof(ParrySpec)} assigned — parry ignored.", this);
                return;
            }

            if (!_parryRegistered && Time.time < _parryCooldownEndsAt)
            {
                return;
            }

            _parryRegistered = false;
            _parryCooldownEndsAt = Time.time + parrySpec.WindowDurationSeconds + parrySpec.MissCooldownSeconds;

            foreach (var member in PlayerControlledDefenders())
            {
                member.BeginParry(parrySpec.WindowDurationSeconds);
                _executors[member].PlayParry();
            }
        }

        // Living player-controlled characters on the side the AI actor opposes.
        private List<CombatParticipant> PlayerControlledDefenders()
        {
            return LivingEnemiesOf(_actor).Where(member => member.IsPlayerControlled).ToList();
        }

        private void OnParrySucceeded(CombatParticipant participant)
        {
            _parryRegistered = true;
            if (_aiActionInProgress && !_parriedEveryHit.ContainsKey(participant))
            {
                _parriedEveryHit[participant] = true;
            }
        }

        private void OnHitNotParried(CombatParticipant participant)
        {
            if (_aiActionInProgress)
            {
                _parriedEveryHit[participant] = false;
            }
        }

        // Once attacker's card has resolved: tells every defender that
        // parried all of its hits (see RecordingPassive).
        private void NotifyPerfectParries(CombatParticipant attacker, SkillCardDefinition card)
        {
            foreach (var entry in _parriedEveryHit)
            {
                if (entry.Value && !entry.Key.IsDefeated)
                {
                    entry.Key.Passives.NotifySkillPerfectlyParried(entry.Key, attacker, card);
                }
            }

            _parriedEveryHit.Clear();
        }
    }
}
