using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GARA.Input
{
    /// <summary>
    /// Maps generic <see cref="InputToken"/>s to concrete Unity Input System actions.
    /// Shared by every input minigame system so none of them need to know how a token
    /// is actually bound.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Input/Input Token Map", fileName = "InputTokenMap")]
    public class InputTokenMap : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public InputToken token;
            public string displayName;
            public InputActionReference action;
        }

        [SerializeField]
        private Entry[] entries = Array.Empty<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryGetAction(InputToken token, out InputAction action)
        {
            foreach (var entry in entries)
            {
                if (entry.token == token && entry.action != null)
                {
                    action = entry.action.action;
                    return action != null;
                }
            }

            action = null;
            return false;
        }

        public bool TryGetDisplayName(InputToken token, out string displayName)
        {
            foreach (var entry in entries)
            {
                if (entry.token == token)
                {
                    displayName = entry.displayName;
                    return true;
                }
            }

            displayName = null;
            return false;
        }

        public void EnableAll()
        {
            foreach (var entry in entries)
            {
                entry.action?.action?.Enable();
            }
        }

        public void DisableAll()
        {
            foreach (var entry in entries)
            {
                entry.action?.action?.Disable();
            }
        }
    }
}
