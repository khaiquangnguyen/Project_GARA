namespace GARA.Characters
{
    public enum StatusEffectKind
    {
        FoodComa
    }

    // One active status inflicted on a participant. Deliberately data-only and
    // generic: the combat loop reads the flags/values, statuses never contain
    // behaviour. Owned and ticked by ICombatTarget's implementor
    // (CombatParticipant), exactly like TimedStatModifier.
    public sealed class StatusEffectInstance
    {
        public StatusEffectKind kind;
        public int remainingTurns;
        public bool skipsTurn;
        public int damagePerSkippedTurn;
        public int bonusDamageTakenPerHit;
        public float incomingDamageMultiplier = 1f;
        public string sourceId;

        public StatusEffectInstance(StatusEffectKind kind, int remainingTurns, string sourceId)
        {
            this.kind = kind;
            this.remainingTurns = remainingTurns;
            this.sourceId = sourceId;
        }

        public bool IsExpired => remainingTurns <= 0;
    }
}
