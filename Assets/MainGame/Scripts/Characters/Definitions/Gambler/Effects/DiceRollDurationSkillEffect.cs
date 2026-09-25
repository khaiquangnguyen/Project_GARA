using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Applies a timed stat modifier whose duration is driven by the dice
    // game's kept roll (highest die) rather than the usual tier-based
    // duration — falls back to a flat duration when the resolution wasn't a
    // dice game (or gamble details aren't available).
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Effects/Dice Roll Duration")]
    public class DiceRollDurationSkillEffect : GambleAwareSkillEffect
    {
        [SerializeField]
        private StatKind stat;

        [SerializeField]
        private float magnitude;

        [SerializeField]
        private int fallbackDurationTurns = 2;

        public override void Resolve(in SkillEffectContext context)
        {
            var duration = fallbackDurationTurns;

            if (TryGetResolution(context.Performance, out var resolution) && resolution.Outcome.GameDetails is DiceRollDetails dice)
            {
                duration = Mathf.Max(1, dice.Kept);
            }

            foreach (var target in context.Targets)
            {
                target.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, duration, name));
            }
        }
    }
}
