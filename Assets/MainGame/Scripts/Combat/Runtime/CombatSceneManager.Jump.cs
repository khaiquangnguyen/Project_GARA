using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Combat
{
    // Jump input during an AI-controlled swing: opens a dodge window and
    // plays each player-controlled defender's JumpState. Always followed by a fixed cooldown, hit or miss.
    // Timings and the jump arc live on JumpSpec.
    public partial class CombatSceneManager
    {
        private InputAction _jump;

        private float _jumpCooldownEndsAt = float.NegativeInfinity;

        private void EnableJump()
        {
            _jump.performed += OnJump;
        }

        private void DisableJump()
        {
            _jump.performed -= OnJump;
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!_aiActionInProgress)
            {
                return;
            }

            if (jumpSpec == null)
            {
                Debug.LogWarning($"[{nameof(CombatSceneManager)}] No {nameof(JumpSpec)} assigned — jump ignored.", this);
                return;
            }

            if (Time.time < _jumpCooldownEndsAt)
            {
                return;
            }

            _jumpCooldownEndsAt = Time.time + jumpSpec.WindowDurationSeconds + jumpSpec.CooldownSeconds;

            foreach (var member in PlayerControlledDefenders())
            {
                member.BeginJump(jumpSpec.WindowDurationSeconds);
                _executors[member].PlayJump(jumpSpec);
            }
        }
    }
}
