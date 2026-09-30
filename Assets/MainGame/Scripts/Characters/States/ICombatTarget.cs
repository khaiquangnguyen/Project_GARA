using System;
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

        // Generic status application (e.g. food coma) — see
        // StatusEffectInstance. Implementation lives on CombatParticipant.
        void ApplyStatus(StatusEffectInstance status);
        bool HasStatus(StatusEffectKind kind);

        // Opponents' target-picking cards skip it / must pick it.
        bool IsUntargetable { get; }
        bool IsTaunting { get; }

        // Removes every active status (or those match picks) and returns
        // them, e.g. to move them onto another character.
        List<StatusEffectInstance> TakeStatuses(Predicate<StatusEffectInstance> match = null);

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
