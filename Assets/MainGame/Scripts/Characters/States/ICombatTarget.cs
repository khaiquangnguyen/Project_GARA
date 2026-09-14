namespace GARA.Characters
{
    // The combat-interaction surface a CharacterState is allowed to
    // see. Implemented by GARA.Combat's CombatParticipant — kept here so
    // GARA.Characters never needs to reference GARA.Combat types directly.
    public interface ICombatTarget
    {
        FactionTag Faction { get; }
        bool IsDefeated { get; }
        void ApplyDamage(int amount);
    }
}
