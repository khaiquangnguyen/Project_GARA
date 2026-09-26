namespace GARA.Characters
{
    // Lets an active passive override how a random min/max range is rolled.
    // Consulted via PassiveRuntimeSet.TryGetRangeRoller.
    public interface IValueRangeRoller
    {
        int RollRange(int minInclusive, int maxInclusive);
        float RollRange(float min, float max);
    }
}
