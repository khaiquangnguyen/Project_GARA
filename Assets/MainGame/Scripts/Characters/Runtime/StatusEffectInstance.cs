using System.Collections.Generic;

namespace GARA.Characters
{
    public enum StatusEffectKind
    {
        FoodComa,
        Noirified,
        Charmed,
        Slowed,
        Weakened,
        Evasive,
        Invulnerable,
        Powered,
        Exorcised,
        Shielded,
        Vulnerable,
        Hardened,
        WellFed,
        Taunting,
        Invisible,
        MethodActing,
        RangeActing
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

        // Scale the Attack / Defense stats (against their base values).
        public float attackMultiplier = 1f;
        public float defenseMultiplier = 1f;

        // Negates the next incoming hit, then is used up (one per stack).
        public bool evadesNextHit;

        // Blocks the next incoming hit, then is used up (one per stack).
        public bool blocksNextHit;

        // Negates every incoming hit while it lasts.
        public bool negatesHits;

        // Scales the damage this character's own skills deal.
        public float outgoingDamageMultiplier = 1f;

        // Set on a charm: the side the character fights for (and whose
        // player/AI picks its cards) while it lasts. Counts down per turn
        // taken charmed, not at turn start.
        public FactionTag? charmedTo;

        // Opponents' cards can't pick this character; hit-all cards still land.
        public bool untargetable;

        // Lost once the character plays a card, takes damage or gains
        // another status.
        public bool endsOnInteraction;

        // Opponents' target-picking cards must pick this character.
        public bool taunts;

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

        public static StatusEffectInstance Charm(FactionTag side, int turns, string sourceId)
        {
            return new StatusEffectInstance(StatusEffectKind.Charmed, turns, sourceId) { charmedTo = side };
        }

        public static StatusEffectInstance Invisible(string sourceId)
        {
            var invisible = Permanent(StatusEffectKind.Invisible, sourceId);
            invisible.untargetable = true;
            invisible.endsOnInteraction = true;
            return invisible;
        }

        public static StatusEffectInstance Taunt(int turns, string sourceId)
        {
            return new StatusEffectInstance(StatusEffectKind.Taunting, turns, sourceId) { taunts = true };
        }

        // Ticks at the end of the owner's turn rather than the start.
        public bool CountsDownAtTurnEnd => charmedTo.HasValue;

        public bool IsExpired => !permanent && remainingTurns <= 0;

        public IEnumerable<StatModifier> GetModifiers()
        {
            if (IsExpired)
            {
                yield break;
            }

            if (speedMultiplier != 1f)
            {
                yield return new StatModifier<SpeedStat> { Mode = ModifierMode.PercentOfBase, Value = speedMultiplier - 1f };
            }

            if (attackMultiplier != 1f)
            {
                yield return new StatModifier<AttackStat> { Mode = ModifierMode.PercentOfBase, Value = attackMultiplier - 1f };
            }

            if (defenseMultiplier != 1f)
            {
                yield return new StatModifier<DefenseStat> { Mode = ModifierMode.PercentOfBase, Value = defenseMultiplier - 1f };
            }
        }
    }
}
