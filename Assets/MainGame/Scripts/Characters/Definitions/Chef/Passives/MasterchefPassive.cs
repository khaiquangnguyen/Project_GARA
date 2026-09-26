using UnityEngine;

namespace GARA.Characters.Chef
{
    // Chef passive: every dish fills its targets' fullness, more per flavor
    // they favor. Filling a target up masters its favorite flavors and puts
    // it in a food coma.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Masterchef", fileName = "MasterchefPassive")]
    public class MasterchefPassive : PassiveDefinition<MasterchefState>
    {
        [Header("Fullness")]
        [Tooltip("Extra fullness per dish flavor the target favors.")]
        [Min(0)]
        [SerializeField] private int matchingPairFullness = 30;

        [Tooltip("Extra fullness per matching pair, per mastery stack of that flavor.")]
        [Min(0)]
        [SerializeField] private int masteryBonusPerStack = 5;

        [Header("Food Coma")]
        [SerializeField] private float foodComaSpeedMultiplier = 0.8f;

        [SerializeField] private float foodComaDamageTakenMultiplier = 1.2f;

        protected override void OnSkillCardResolved(in PassiveContext context, MasterchefState state)
        {
            if (!(context.Card is ChefSkillCard card) || context.CardTargets == null)
            {
                return;
            }

            // Nothing got cooked, nothing to eat.
            if (context.Performance.TryGetDetails<DishReport>(out var dish) && dish.Source != null && !DishCleared.AnyStep(dish.Source))
            {
                return;
            }

            foreach (var target in context.CardTargets)
            {
                var palate = target.Palate;
                if (!palate.CanBeFed)
                {
                    continue;
                }

                var result = target.Feed(FullnessFor(card, palate, state));
                if (!result.becameFull)
                {
                    continue;
                }

                foreach (var flavor in palate.FavoriteFlavors.Split())
                {
                    state.AddMastery(flavor);
                }

                if (!target.HasStatus(StatusEffectKind.FoodComa))
                {
                    target.ApplyStatus(CreateFoodComa());
                }
            }
        }

        public int FullnessFor(ChefSkillCard card, PalateProfile palate, MasterchefState state)
        {
            var fullness = card.BaseFullness;
            var favorites = palate.FavoriteFlavors;
            foreach (var flavor in (card.Flavors & favorites).Split())
            {
                fullness += PairFullness(state, flavor);
            }

            // Umami pairs with any favorite; it takes the best-mastered one.
            if ((card.Flavors & FlavorTag.Umami) != 0 && favorites != FlavorTag.None)
            {
                var best = 0;
                foreach (var flavor in favorites.Split())
                {
                    best = Mathf.Max(best, PairFullness(state, flavor));
                }

                fullness += best;
            }

            return fullness;
        }

        private int PairFullness(MasterchefState state, FlavorTag flavor)
        {
            return matchingPairFullness + masteryBonusPerStack * state.MasteryOf(flavor);
        }

        private StatusEffectInstance CreateFoodComa()
        {
            var coma = StatusEffectInstance.Permanent(StatusEffectKind.FoodComa, passiveId);
            coma.speedMultiplier = foodComaSpeedMultiplier;
            coma.incomingDamageMultiplier = foodComaDamageTakenMultiplier;
            return coma;
        }
    }
}
