using System;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // One bullet, spun once; the target and the Gambler take turns pulling.
    [Serializable]
    public class DuelRouletteGame : GamblingGame<DuelRouletteOutcome>
    {
        [Min(2)]
        public int chambers = 6;

        [Tooltip("The target chickens out once the next pull's fire chance reaches this.")]
        [Range(0, 1)]
        public float targetBailAt = 0.5f;

        protected override DuelRouletteOutcome RollTyped(bool cheated, IGambleRandom rng)
        {
            // Cheat: never in the Gambler's first chamber (pull 1).
            var bullet = cheated ? rng.Range(0, chambers - 1) : rng.Range(0, chambers);
            if (cheated && bullet >= 1)
            {
                bullet++;
            }

            return new DuelRouletteOutcome(chambers, bullet);
        }

        // The target's call on its turn.
        public bool TargetPulls(DuelRouletteOutcome round)
        {
            return !round.CanChickenOut(DuelSeat.Target) || round.NextFireChance < targetBailAt;
        }
    }
}
