using UnityEngine;

namespace GARA.Characters.Exorcist
{
    // Exorcist passive: every negative status an opponent gains (from anyone)
    // brands it with Seals, as do cards that seal directly (AddSealsEffect). At max Seals it's exorcised — its demon soul torn
    // out — stunned, and takes extra damage until it's back on its feet.
    [CreateAssetMenu(menuName = "GARA/Characters/Passives/Rite of Exorcism", fileName = "RiteOfExorcismPassive")]
    public class RiteOfExorcismPassive : PassiveDefinition<RiteOfExorcismState>
    {
        [Tooltip("Seals added per negative status an opponent gains (or has extended).")]
        [Min(1)]
        [SerializeField] private int sealsPerNegativeStatus = 1;

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

        // No-op when self has no Rite of Exorcism passive.
        public static void AddSeals(ICombatTarget self, ICombatTarget target, int amount)
        {
            if (TryFind(self, out var rite, out var state))
            {
                rite.Seal(self, target, amount, state);
            }
        }

        protected override void OnStatusApplied(ICombatTarget self, ICombatTarget target, StatusEffectInstance status, RiteOfExorcismState state)
        {
            if (status.kind.IsNegative())
            {
                Seal(self, target, sealsPerNegativeStatus, state);
            }
        }

        private void Seal(ICombatTarget self, ICombatTarget target, int amount, RiteOfExorcismState state)
        {
            if (target == null || target.IsDefeated || target.Faction == self.Faction || target.HasStatus(StatusEffectKind.Exorcised))
            {
                return;
            }

            var seals = state.AddSeals(target, amount);
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

        private static bool TryFind(ICombatTarget character, out RiteOfExorcismPassive rite, out RiteOfExorcismState state)
        {
            var passives = character?.Passives;
            if (passives != null)
            {
                foreach (var passive in passives.Passives)
                {
                    if (passive is RiteOfExorcismPassive found && passives.GetState(passive) is RiteOfExorcismState foundState)
                    {
                        rite = found;
                        state = foundState;
                        return true;
                    }
                }
            }

            rite = null;
            state = null;
            return false;
        }
    }
}
