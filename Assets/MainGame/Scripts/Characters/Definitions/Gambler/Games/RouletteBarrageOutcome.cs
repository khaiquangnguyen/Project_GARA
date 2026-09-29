using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    public sealed class RouletteBarrageOutcome : GambleOutcome
    {
        public readonly IReadOnlyList<bool> Fired;
        public readonly int Chambers;
        public readonly int Loaded;
        public readonly int Hits;

        public RouletteBarrageOutcome(IReadOnlyList<bool> fired, int chambers, int loaded)
        {
            Fired = fired;
            Chambers = chambers;
            Loaded = loaded;
            foreach (var shot in fired)
            {
                if (shot)
                {
                    Hits++;
                }
            }
        }

        public override float Significance => Fired.Count > 0 ? Hits / (float)Fired.Count : 0f;

        public override string ToString()
        {
            return $"Roulette Barrage: {Hits}/{Fired.Count} fired ({Loaded}/{Chambers} loaded)";
        }
    }
}
