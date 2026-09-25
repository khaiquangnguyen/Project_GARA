namespace GARA.Countdowns
{
    /// <summary>
    /// How the stack picks which countdown a single press resolves when several are active.
    /// </summary>
    public enum CountdownPriority
    {
        SoonestToExpire,
        OldestSpawned,
        NewestSpawned
    }
}
