using System;

namespace GARA.InputCombos
{
    /// <summary>
    /// Generic effect payload for a combo step. The engine never reads this — it only hands
    /// it back to the consumer via <see cref="Runtime.ComboStepAccepted"/>.
    /// </summary>
    [Serializable]
    public struct ComboStepPayload
    {
        public string effectId;
        public float magnitude;
        public int count;
        public string data;
        public UnityEngine.Object asset;
    }
}
