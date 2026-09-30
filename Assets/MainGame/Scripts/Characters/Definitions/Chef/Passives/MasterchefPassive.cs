using UnityEngine;

namespace GARA.Characters.Chef
{
    // Chef passive: healing or newly buffing allies, and hitting or newly
    // debuffing enemies, fills satiety. A full bar grants a Masterchef stack
    // (a random stat up) and feeds whoever filled it: Well Fed for an ally,
    // Food Coma for an enemy.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Masterchef", fileName = "MasterchefPassive")]
    public class MasterchefPassive : PassiveDefinition<MasterchefState>
    {
        [Header("Satiety")]
        [Min(1)]
        [SerializeField] private int satietyCapacity = 100;

        [Min(0)]
        [SerializeField] private int satietyPerHeal = 15;

        [Tooltip("Per status an ally didn't have yet.")]
        [Min(0)]
        [SerializeField] private int satietyPerBuff = 25;

        [Tooltip("Per hit landed on an enemy.")]
        [Min(0)]
        [SerializeField] private int satietyPerHit = 10;

        [Tooltip("Per status an enemy didn't have yet.")]
        [Min(0)]
        [SerializeField] private int satietyPerDebuff = 25;

        [Header("Masterchef")]
        [Tooltip("Each stack raises one of these, picked at random.")]
        [SerializeField] private MasterchefStatGain[] statGains =
        {
            new MasterchefStatGain { stat = StatKind.Attack, amount = 2f },
            new MasterchefStatGain { stat = StatKind.Defense, amount = 2f },
            new MasterchefStatGain { stat = StatKind.Speed, amount = 2f },
            new MasterchefStatGain { stat = StatKind.MaxHp, amount = 10f }
        };

        [Header("Food Coma (enemy)")]
        [Min(1)]
        [SerializeField] private int foodComaTurns = 2;

        [SerializeField] private float foodComaSpeedMultiplier = 0.8f;

        [SerializeField] private float foodComaDamageTakenMultiplier = 1.2f;

        [Header("Well Fed (ally)")]
        [Min(1)]
        [SerializeField] private int wellFedTurns = 2;

        [SerializeField] private float wellFedSpeedMultiplier = 1.2f;

        [SerializeField] private float wellFedDamageTakenMultiplier = 0.8f;

        protected override void OnSkillCardResolved(in PassiveContext context, MasterchefState state)
        {
        }

        protected override void OnHealed(ICombatTarget self, ICombatTarget target, int amount, MasterchefState state)
        {
            if (IsAlly(self, target))
            {
                Fill(self, target, satietyPerHeal, state);
            }
        }

        protected override void OnHitLanded(ICombatTarget self, ICombatTarget target, int damage, MasterchefState state)
        {
            if (!IsAlly(self, target))
            {
                Fill(self, target, satietyPerHit, state);
            }
        }

        protected override void OnStatusInflicted(ICombatTarget self, ICombatTarget target, StatusEffectInstance status, bool isNew, MasterchefState state)
        {
            // Its own Well Fed / Food Coma never refills the bar.
            if (!isNew || status.sourceId == passiveId)
            {
                return;
            }

            var ally = IsAlly(self, target);
            if (ally && !status.kind.IsNegative())
            {
                Fill(self, target, satietyPerBuff, state);
            }
            else if (!ally && status.kind.IsNegative())
            {
                Fill(self, target, satietyPerDebuff, state);
            }
        }

        private static bool IsAlly(ICombatTarget self, ICombatTarget target)
        {
            return target.Faction == self.Faction;
        }

        private void Fill(ICombatTarget self, ICombatTarget target, int amount, MasterchefState state)
        {
            if (amount <= 0 || !state.AddSatiety(amount, satietyCapacity))
            {
                return;
            }

            if (statGains.Length > 0)
            {
                var gain = statGains[Random.Range(0, statGains.Length)];
                self.ApplyTimedModifier(TimedStatModifier.Permanent(gain.stat, gain.amount, passiveId));
            }

            if (!target.IsDefeated)
            {
                target.ApplyStatus(IsAlly(self, target) ? CreateWellFed() : CreateFoodComa());
            }
        }

        private StatusEffectInstance CreateFoodComa()
        {
            return new StatusEffectInstance(StatusEffectKind.FoodComa, foodComaTurns, passiveId)
            {
                speedMultiplier = foodComaSpeedMultiplier,
                incomingDamageMultiplier = foodComaDamageTakenMultiplier
            };
        }

        private StatusEffectInstance CreateWellFed()
        {
            return new StatusEffectInstance(StatusEffectKind.WellFed, wellFedTurns, passiveId)
            {
                speedMultiplier = wellFedSpeedMultiplier,
                incomingDamageMultiplier = wellFedDamageTakenMultiplier
            };
        }
    }
}
