using System;
using UnityEngine.Serialization;

namespace GARA.Characters
{
    // Inspector-authored base numbers. Data only: it exists so designers can
    // edit base stats on a ScriptableObject, and is converted into a live
    // StatBlock at runtime.
    [Serializable]
    public struct BaseStatValues
    {

        public int maxHp;

        public int maxMp;

        public int maxAp;

        public int attack;

        public int defense;

        public int speed;

        public StatBlock CreateStats()
        {
            return new StatBlock(maxHp, maxMp, maxAp, attack, defense, speed);
        }
    }
}
