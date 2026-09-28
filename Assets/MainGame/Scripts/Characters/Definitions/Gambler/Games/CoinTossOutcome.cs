using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    // Every coin of one toss; true is heads.
    public sealed class CoinTossOutcome : GambleOutcome
    {
        public readonly IReadOnlyList<bool> Faces;
        public readonly int HeadsCount;

        public CoinTossOutcome(IReadOnlyList<bool> faces)
        {
            Faces = faces;
            foreach (var heads in faces)
            {
                if (heads)
                {
                    HeadsCount++;
                }
            }
        }

        public override float Significance => Faces.Count > 0 ? HeadsCount / (float)Faces.Count : 0f;

        public override string ToString()
        {
            return $"Coins: {HeadsCount}/{Faces.Count} heads";
        }
    }
}
