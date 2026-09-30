namespace GARA.Characters
{
    public static class StatusEffectKindExtensions
    {
        // Harmful to whoever has it (what a cleanse removes).
        public static bool IsNegative(this StatusEffectKind kind)
        {
            return kind is StatusEffectKind.FoodComa
                or StatusEffectKind.Noirified
                or StatusEffectKind.Charmed
                or StatusEffectKind.Slowed
                or StatusEffectKind.Weakened
                or StatusEffectKind.Exorcised
                or StatusEffectKind.Vulnerable;
        }

        // Tied to its owner's passive, so never moved to another character.
        public static bool IsTransferable(this StatusEffectKind kind)
        {
            return kind is not (StatusEffectKind.MethodActing or StatusEffectKind.RangeActing);
        }

        // Reapplying adds its turns to the active one instead of stacking
        // its effect.
        public static bool ExtendsOnReapply(this StatusEffectKind kind)
        {
            return kind is StatusEffectKind.Weakened
                or StatusEffectKind.Vulnerable
                or StatusEffectKind.Hardened
                or StatusEffectKind.FoodComa
                or StatusEffectKind.WellFed
                or StatusEffectKind.Taunting;
        }
    }
}
