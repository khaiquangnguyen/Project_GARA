namespace GARA.Characters
{
    public class MaxHpStat : Stat
    {
        public MaxHpStat(int baseValue) : base(baseValue) { }

        public override string DisplayName => "HP";
    }

    public class MaxMpStat : Stat
    {
        public MaxMpStat(int baseValue) : base(baseValue) { }

        public override string DisplayName => "MP";
    }

    public class MaxApStat : Stat
    {
        public MaxApStat(int baseValue) : base(baseValue) { }

        public override string DisplayName => "AP";
    }

    public class AttackStat : Stat
    {
        public AttackStat(int baseValue) : base(baseValue) { }

        public override string DisplayName => "Attack";
    }

    public class DefenseStat : Stat
    {
        public DefenseStat(int baseValue) : base(baseValue) { }

        public override string DisplayName => "Defense";
    }

    public class SpeedStat : Stat
    {
        public SpeedStat(int baseValue) : base(baseValue) { }

        public override string DisplayName => "Speed";
    }
}
