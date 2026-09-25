using GARA.Characters;
using UnityEngine;

namespace GARA.Characters.Chef
{
    // Cooking (Chef): feeds every target fullness based on how well the dish
    // matches their favorite flavors, scaled by tier and by how well the
    // recipe came out. A target that fills up goes into a food coma (a
    // turn-skipping StatusEffectInstance) — see CombatParticipant.Feed.
    [CreateAssetMenu(menuName = "GARA/Skill Cards/Chef/Effects/Feed Fullness", fileName = "FeedFullnessSkillEffect")]
    public class FeedFullnessSkillEffect : SkillEffectDefinition
    {
        [SerializeField] private int baseFullness = 2;
        [SerializeField] private StatScaling matchBonusByMatchCount;
        [SerializeField] private int umamiBonus = 2;
        [SerializeField] private TierScaling tierMultiplier;
        [SerializeField] private float burntMultiplier = 0.5f;
        [SerializeField] private int comaTurns = 2;
        [SerializeField] private int comaDamagePerTurn = 2;
        [SerializeField] private int comaBonusDamagePerHit;
        [SerializeField] private float comaIncomingDamageMultiplier = 1f;

        public override void Resolve(in SkillEffectContext context)
        {
            var flavors = FlavorTag.None;
            var anyBurnt = false;
            if (context.Performance.TryGetDetails<DishReport>(out var dish))
            {
                flavors = dish.Flavors;
                anyBurnt = dish.AnyBurnt;
            }

            var tierMult = tierMultiplier.For(context.Performance.Tier);
            if (anyBurnt)
            {
                tierMult *= burntMultiplier;
            }

            foreach (var target in context.Targets)
            {
                var palate = target.Palate;
                if (!palate.CanBeFed)
                {
                    continue;
                }

                var matches = palate.CountMatches(flavors);
                var gain = Mathf.Max(0, Mathf.RoundToInt((baseFullness + matchBonusByMatchCount.Evaluate(matches) + (flavors.HasUmami() ? umamiBonus : 0)) * tierMult));
                var result = target.Feed(gain);
                if (result.becameFull)
                {
                    target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.FoodComa, comaTurns, name)
                    {
                        skipsTurn = true,
                        damagePerSkippedTurn = comaDamagePerTurn,
                        bonusDamageTakenPerHit = comaBonusDamagePerHit,
                        incomingDamageMultiplier = comaIncomingDamageMultiplier
                    });
                }
            }
        }
    }
}
