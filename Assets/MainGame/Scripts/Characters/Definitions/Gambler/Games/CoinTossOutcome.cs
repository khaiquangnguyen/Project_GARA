namespace GARA.Characters.Gambler
{
    public sealed class CoinTossOutcome : GambleOutcome
    {
        public readonly bool Heads;

        public CoinTossOutcome(bool heads)
        {
            Heads = heads;
        }

        public override float Significance => Heads ? 1f : 0f;

        public override string ToString()
        {
            return $"Coin: {(Heads ? "Heads" : "Tails")}";
        }
    }
}
