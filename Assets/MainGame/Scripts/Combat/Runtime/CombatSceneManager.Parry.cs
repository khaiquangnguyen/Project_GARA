using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // Parry input during an enemy swing: opens a parry window on every living
    // player and plays their ParryState. A missed window is followed by a
    // cooldown; a successful parry allows an immediate retry. Timings live on
    // ParrySpec.
    public partial class CombatSceneManager
    {
        private InputAction _parry;
        private bool _enemyActionInProgress;

        private float _parryCooldownEndsAt = float.NegativeInfinity;
        private bool _parryRegistered;

        private void EnableParry()
        {
            _parry.performed += OnParry;
            CombatParticipant.ParrySucceeded += OnParrySucceeded;
        }

        private void DisableParry()
        {
            _parry.performed -= OnParry;
            CombatParticipant.ParrySucceeded -= OnParrySucceeded;
        }

        private void OnParry(InputAction.CallbackContext ctx)
        {
            if (!_enemyActionInProgress)
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

            foreach (var member in _battle.playerParty.LivingMembers().ToList())
            {
                member.BeginParry(parrySpec.WindowDurationSeconds);
                _executors[member].PlayParry();
            }
        }

        private void OnParrySucceeded(CombatParticipant participant)
        {
            _parryRegistered = true;
        }
    }
}
