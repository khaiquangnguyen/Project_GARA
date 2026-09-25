namespace GARA.Characters
{
    // Lets an active passive override how a random min/max range is rolled
    // (e.g. Gambler's Gambling Addict forcing favorable rolls). Consulted
    // via PassiveRuntimeSet.TryGetRangeRoller wherever a skill effect would
    // otherwise roll its own range.
    public interface IValueRangeRoller
    {
        int RollRange(int minInclusive, int maxInclusive);
        float RollRange(float min, float max);
    }
}
