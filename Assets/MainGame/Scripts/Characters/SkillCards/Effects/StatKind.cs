namespace GARA.Characters
{
    // Mirrors the concrete Stat subclasses StatBlock exposes today
    // (MaxHpStat, MaxMpStat, MaxApStat, AttackStat, DefenseStat, SpeedStat).
    // Effects reference a stat by this enum (Inspector-friendly) rather than
    // by Type.
    public enum StatKind
    {
        MaxHp,
        MaxMp,
        MaxAp,
        Attack,
        Defense,
        Speed
    }
}
