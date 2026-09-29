using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // One face on the slot machine's reels.
    [Serializable]
    public class SlotSymbol
    {
        public string symbolName;

        public Sprite icon;

        [Tooltip("Relative odds of a reel landing here; 0 never lands.")]
        [Min(0)]
        public int weight = 1;

        [Tooltip("Paid once per reel that lands here.")]
        [SerializeReference]
        [SubclassPicker]
        public SlotReward reward;

        [Tooltip("Paid instead, once, when every reel lands here.")]
        [SerializeReference]
        [SubclassPicker]
        public SlotReward jackpotReward;
    }
}
