using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // One weighted symbol a slot machine reel can land on.
    [Serializable]
    public struct SlotSymbol
    {
        public string symbolId;
        public Sprite icon;
        public float weight;
    }
}
