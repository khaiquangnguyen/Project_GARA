using System.Collections.Generic;

namespace GARA.Characters
{
    public enum StatusEffectKind
    {
        FoodComa,
        Noirified
    }

    // One active status inflicted on a participant. Deliberately data-only and
    // generic: the combat loop reads the flags/values, statuses never contain
    // behaviour. Owned and ticked by ICombatTarget's implementor
    // (CombatParticipant), exactly like TimedStatModifier.
    public sealed class StatusEffectInstance : IStatModifierSource
    {
        public StatusEffectKind kind;
        public int remainingTurns;
        public bool permanent;
        public bool skipsTurn;
        public int damagePerSkippedTurn;
        public int bonusDamageTakenPerHit;
        public float incomingDamageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public string sourceId;

        public StatusEffectInstance(StatusEffectKind kind, int remainingTurns, string sourceId)
        {
            this.kind = kind;
            this.remainingTurns = remainingTurns;
            this.sourceId = sourceId;
        }

        // Lasts until the battle ends; never ticks down.
        public static StatusEffectInstance Permanent(StatusEffectKind kind, string sourceId)
        {
            return new StatusEffectInstance(kind, 0, sourceId) { permanent = true };
        }

        public bool IsExpired => !permanent && remainingTurns <= 0;

        public IEnumerable<StatModifier> GetModifiers()
        {
            if (!IsExpired && speedMultiplier != 1f)
            {
                yield return new StatModifier<SpeedStat> { Mode = ModifierMode.PercentOfBase, Value = speedMultiplier - 1f };
            }
        }
    }
}
