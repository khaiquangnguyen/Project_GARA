using UnityEngine;

namespace GARA.InputSets
{
    /// <summary>
    /// An ordered list of <see cref="InputSetDefinition"/> sets the player must clear in order.
    /// Getting an input wrong, or timing out, resets only the current set.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Input Sets/Input Set Collection", fileName = "InputSetCollection")]
    public class InputSetCollectionDefinition : ScriptableObject
    {
        [SerializeField]
        private InputSetDefinition[] sets = System.Array.Empty<InputSetDefinition>();

        [SerializeField]
        private float totalTimeLimit;

        [SerializeField]
        private InputSetRetryPolicy retryPolicy = InputSetRetryPolicy.Default;

        public InputSetDefinition[] Sets => sets;

        public float TotalTimeLimit => totalTimeLimit;

        public InputSetRetryPolicy RetryPolicy => retryPolicy;

        /// <summary>Builds an in-memory set collection (not saved as an asset) — for tests and procedurally generated set collections. Caller owns destroying it.</summary>
        public static InputSetCollectionDefinition CreateRuntime(InputSetDefinition[] sets, float totalTimeLimit, InputSetRetryPolicy retryPolicy)
        {
            var definition = CreateInstance<InputSetCollectionDefinition>();
            definition.sets = sets;
            definition.totalTimeLimit = totalTimeLimit;
            definition.retryPolicy = retryPolicy;
            return definition;
        }

        public int TotalInputs
        {
            get
            {
                var total = 0;
                foreach (var set in sets)
                {
                    if (set.inputs != null)
                    {
                        total += set.inputs.Length;
                    }
                }

                return total;
            }
        }

        private void OnValidate()
        {
            if (sets == null || sets.Length == 0)
            {
                Debug.LogWarning($"{name}: InputSetCollectionDefinition has no sets.", this);
                return;
            }

            for (var i = 0; i < sets.Length; i++)
            {
                if (sets[i].inputs == null || sets[i].inputs.Length == 0)
                {
                    Debug.LogWarning($"{name}: set {i} has an empty inputs array.", this);
                }
            }
        }
    }
}
