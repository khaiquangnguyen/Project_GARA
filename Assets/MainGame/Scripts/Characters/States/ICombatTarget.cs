namespace GARA.Characters
{
    // The combat-interaction surface a CharacterState is allowed to
    // see. Implemented by GARA.Combat's CombatParticipant — kept here so
    // GARA.Characters never needs to reference GARA.Combat types directly.
    public interface ICombatTarget
    {
        FactionTag Faction { get; }
        bool IsDefeated { get; }
        StatBlock CurrentStats { get; }
        void ApplyDamage(int amount);

        // Implementation lives on CombatParticipant (GARA.Combat) — out of
        // scope here. Declared on the interface so GARA.Characters-side
        // skill effects can attach a buff/debuff without depending on
        // GARA.Combat.
        void ApplyTimedModifier(TimedStatModifier modifier);

        // Per-participant class-passive runtime state. Implementation lives
        // on CombatParticipant (GARA.Combat) — out of scope here. Declared
        // on the interface so GARA.Characters-side passives/effects can
        // read/notify a participant's passives without depending on
        // GARA.Combat.
        PassiveRuntimeSet Passives { get; }

        // Cooking (Chef): whether/how this target can be fed, and its
        // current fullness. Implementation lives on CombatParticipant
        // (GARA.Combat) — declared here so GARA.Characters-side skill
        // effects (e.g. FeedFullnessSkillEffect) can feed a target without
        // depending on GARA.Combat.
        PalateProfile Palate { get; }
        int Fullness { get; }
        FeedResult Feed(int amount);

        // Generic status application (e.g. the food-coma stun) — see
        // StatusEffectInstance. Implementation lives on CombatParticipant.
        void ApplyStatus(StatusEffectInstance status);
    }
}
