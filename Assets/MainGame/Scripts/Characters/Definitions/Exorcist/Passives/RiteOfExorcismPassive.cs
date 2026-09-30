using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Exorcist passive: every hit on an opponent brands it with Seals. At max
    // Seals it's exorcised — its demon soul torn out — stunned, and takes
    // extra damage until it's back on its feet.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Rite of Exorcism", fileName = "RiteOfExorcismPassive")]
    public class RiteOfExorcismPassive : PassiveDefinition<RiteOfExorcismState>
    {
        [Tooltip("Seals added per hit landed.")]
        [Min(1)]
        [SerializeField] private int sealsPerHit = 1;

        [Tooltip("Seals that exorcise an opponent.")]
        [Min(1)]
        [SerializeField] private int maxSeals = 6;

        [Tooltip("Turns (the exorcised character's own) it's stunned.")]
        [Min(1)]
        [SerializeField] private int stunTurns = 1;

        [Tooltip("Damage multiplier on every hit it takes while exorcised: through the stun and the whole turn after it.")]
        [Min(1f)]
        [SerializeField] private float damageTakenMultiplier = 1.5f;

        public int MaxSeals => maxSeals;

        protected override void OnHitLanded(ICombatTarget self, ICombatTarget target, int damage, RiteOfExorcismState state)
        {
            if (target.IsDefeated || target.Faction == self.Faction || target.HasStatus(StatusEffectKind.Exorcised))
            {
                return;
            }

            var seals = state.AddSeals(target, sealsPerHit);
            Debug.Log($"[{nameof(RiteOfExorcismPassive)}] seals {Mathf.Min(seals, maxSeals)}/{maxSeals}.");
            if (seals < maxSeals)
            {
                return;
            }

            state.ClearSeals(target);
            Exorcise(target);
        }

        // Two statuses of one kind: the stun burns on the skipped turns, while
        // the exposure ticks at turn start, so it lasts one turn longer.
        private void Exorcise(ICombatTarget target)
        {
            target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Exorcised, stunTurns, passiveId) { skipsTurn = true });
            target.ApplyStatus(new StatusEffectInstance(StatusEffectKind.Exorcised, stunTurns + 1, passiveId) { incomingDamageMultiplier = damageTakenMultiplier });
        }

        protected override void OnSkillCardResolved(in PassiveContext context, RiteOfExorcismState state)
        {
        }
    }
}
