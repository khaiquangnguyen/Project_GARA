using GARA.InputSets;
using UnityEngine;

namespace GARA.Chords
{
    /// <summary>
    /// An ordered list of <see cref="ChordDefinition"/> chords the player must clear in order.
    /// Reuses <see cref="InputSets"/>'s retry policy, outcome and report types wholesale.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Chords/Chord Sequence", fileName = "ChordSequence")]
    public class ChordSequenceDefinition : ScriptableObject
    {
        [SerializeField]
        private ChordDefinition[] chords = System.Array.Empty<ChordDefinition>();

        [SerializeField]
        private float defaultSimultaneityWindow = 0.1f;

        [SerializeField]
        private float totalTimeLimit;

        [SerializeField]
        private InputSetRetryPolicy retryPolicy = InputSetRetryPolicy.Default;

        [SerializeField]
        private bool extraInputFailsChord = true;

        [SerializeField]
        private bool requireAllReleasedBeforeRetry = true;

        public ChordDefinition[] Chords => chords;

        public float DefaultSimultaneityWindow => defaultSimultaneityWindow;

        public float TotalTimeLimit => totalTimeLimit;

        public InputSetRetryPolicy RetryPolicy => retryPolicy;

        public bool ExtraInputFailsChord => extraInputFailsChord;

        public bool RequireAllReleasedBeforeRetry => requireAllReleasedBeforeRetry;

        public float WindowFor(int chordIndex)
        {
            return chords[chordIndex].simultaneityWindow > 0f
                ? chords[chordIndex].simultaneityWindow
                : defaultSimultaneityWindow;
        }

        private void OnValidate()
        {
            if (chords == null || chords.Length == 0)
            {
                Debug.LogWarning($"{name}: ChordSequenceDefinition has no chords.", this);
                return;
            }

            for (var i = 0; i < chords.Length; i++)
            {
                if (chords[i].required == null || chords[i].required.Length == 0)
                {
                    Debug.LogWarning($"{name}: chord {i} has an empty required array.", this);
                }

                var window = chords[i].simultaneityWindow > 0f ? chords[i].simultaneityWindow : defaultSimultaneityWindow;
                if (window < 0.03f)
                {
                    Debug.LogWarning($"{name}: chord {i} has a simultaneity window of {window:F3}s, which is below one frame.", this);
                }
            }
        }
    }
}
