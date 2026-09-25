using System;

namespace GARA.Input
{
    /// <summary>
    /// Plain C# helper a MonoBehaviour driver calls from Update() to turn an
    /// <see cref="InputTokenMap"/> into press/release callbacks, mirroring the
    /// project's existing per-frame Input System polling convention.
    /// </summary>
    public sealed class InputTokenPoller
    {
        private readonly InputTokenMap _map;

        public InputTokenPoller(InputTokenMap map)
        {
            _map = map;
        }

        public void Poll(Action<InputToken> onPressed, Action<InputToken> onReleased)
        {
            if (_map == null)
            {
                return;
            }

            foreach (var entry in _map.Entries)
            {
                var action = entry.action != null ? entry.action.action : null;
                if (action == null)
                {
                    continue;
                }

                if (action.WasPressedThisFrame())
                {
                    onPressed?.Invoke(entry.token);
                }

                if (action.WasReleasedThisFrame())
                {
                    onReleased?.Invoke(entry.token);
                }
            }
        }

        public bool IsHeld(InputToken token)
        {
            return _map != null && _map.TryGetAction(token, out var action) && action.IsPressed();
        }
    }
}
