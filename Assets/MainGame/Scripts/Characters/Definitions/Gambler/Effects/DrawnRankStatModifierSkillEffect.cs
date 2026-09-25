using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Gambler
{
    // Applies a timed stat modifier scaled by how high the drawn card's rank
    // was (rank 14 == Ace == full baseMagnitude), falling back to the flat
    // baseMagnitude when the resolution wasn't a card game.
    [CreateAssetMenu(menuName = "GARA/Characters/Gambler/Effects/Drawn Rank Stat Modifier")]
    public class DrawnRankStatModifierSkillEffect : GambleAwareSkillEffect
    {
        [SerializeField]
        private StatKind stat;

        [SerializeField]
        private float baseMagnitude;

        [SerializeField]
        private int durationTurns = 2;

        public override void Resolve(in SkillEffectContext context)
        {
            var magnitude = baseMagnitude;

            if (TryGetResolution(context.Performance, out var resolution) && resolution.Outcome.GameDetails is CardDrawDetails card)
            {
                magnitude = baseMagnitude * (card.Drawn.Rank / 14f);
            }

            foreach (var target in context.Targets)
            {
                target.ApplyTimedModifier(new TimedStatModifier(stat, magnitude, durationTurns, name));
            }
        }
    }
}
