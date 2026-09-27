using System.Collections.Generic;

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

        // Restores HP up to max; never revives the defeated.
        void Heal(int amount);

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
        // (GARA.Combat) — declared here so the Chef's Masterchef passive
        // can feed a target without depending on GARA.Combat.
        PalateProfile Palate { get; }
        int Fullness { get; }
        FeedResult Feed(int amount);

        // Generic status application (e.g. food coma) — see
        // StatusEffectInstance. Implementation lives on CombatParticipant.
        void ApplyStatus(StatusEffectInstance status);
        bool HasStatus(StatusEffectKind kind);

        // What this character is and can play — lets a passive copy its
        // skills (see FormReplaySkillCard). Implementation lives on
        // CombatParticipant.
        CharacterDefinition Definition { get; }
        IReadOnlyList<SkillCardDefinition> SkillCards { get; }

        // The last card it played this battle, or null.
        SkillCardDefinition LastUsedSkillCard { get; }

        // Adds a card to this character's hand for the rest of the battle
        // (or until played, for a one-time card).
        void AddSkillCard(SkillCardDefinition card);
        bool RemoveSkillCard(SkillCardDefinition card);
    }
}
