using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    public sealed class DiceOutcome : GambleOutcome
    {
        public readonly IReadOnlyList<int> Faces;
        public readonly int Sides;
        public readonly int Total;

        public DiceOutcome(IReadOnlyList<int> faces, int sides)
        {
            Faces = faces;
            Sides = sides;
            foreach (var face in faces)
            {
                Total += face;
            }
        }

        public override float Significance
        {
            get
            {
                var min = Faces.Count;
                var max = Faces.Count * Sides;
                return max > min ? (Total - min) / (float)(max - min) : 1f;
            }
        }

        public override string ToString()
        {
            return $"Dice: [{string.Join(", ", Faces)}] = {Total}";
        }
    }
}
