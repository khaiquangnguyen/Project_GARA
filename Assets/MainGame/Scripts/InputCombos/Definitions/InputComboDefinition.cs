using System;
using UnityEngine;

namespace GARA.InputCombos
{
    /// <summary>
    /// Authoring asset for a session-long input combo: ordered steps, timing mode, and scoring.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Input Combos/Input Combo", fileName = "InputCombo")]
    public class InputComboDefinition : ScriptableObject
    {
        public ComboStepDefinition[] steps = Array.Empty<ComboStepDefinition>();
        public ComboTimingMode timingMode = ComboTimingMode.FromFirstInput;
        public float comboTimeAllowed = 1f;
        public float completionBonus;
        public bool restartOnResetInput = true;
        public float sessionDuration;

        private void OnValidate()
        {
            if (steps.Length == 0)
            {
                Debug.LogWarning($"InputComboDefinition '{name}': steps is empty.", this);
            }

            if (timingMode == ComboTimingMode.FromFirstInput)
            {
                if (comboTimeAllowed <= 0f)
                {
                    Debug.LogWarning($"InputComboDefinition '{name}': comboTimeAllowed must be > 0 in FromFirstInput mode.", this);
                }

                foreach (var step in steps)
                {
                    if (step.timeAllowed > 0f)
                    {
                        Debug.LogWarning($"InputComboDefinition '{name}': a step has timeAllowed set, but it is ignored in FromFirstInput mode.", this);
                        break;
                    }
                }
            }
        }
    }
}
