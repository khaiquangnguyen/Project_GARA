namespace GARA.Characters.Gambler
{
    // One Duel Roulette round, played pull by pull (target first, then
    // turns) until someone is shot or chickens out. The bullet's chamber is
    // fixed when it's rolled.
    public sealed class DuelRouletteOutcome : GambleOutcome
    {
        public readonly int Chambers;
        public readonly int BulletChamber;

        public int Pulls { get; private set; }

        public DuelEnding Ending { get; private set; }

        // Who was shot or chickened out.
        public DuelSeat EndedBy { get; private set; }

        public DuelRouletteOutcome(int chambers, int bulletChamber)
        {
            Chambers = chambers;
            BulletChamber = bulletChamber;
        }

        public bool IsOver => Ending != DuelEnding.None;

        public int EmptyClicks => Ending == DuelEnding.Shot ? Pulls - 1 : Pulls;

        public DuelSeat NextSeat => SeatOfPull(Pulls);

        // Chance the next pull fires, as far as anyone at the table knows.
        public float NextFireChance => 1f / (Chambers - Pulls);

        public override float Significance
        {
            get
            {
                if (!IsOver)
                {
                    return 0f;
                }

                if (EndedBy == DuelSeat.Target)
                {
                    return Ending == DuelEnding.Shot ? 1f : 0.75f;
                }

                return Ending == DuelEnding.Shot ? 0f : 0.25f;
            }
        }

        public static DuelSeat SeatOfPull(int pull)
        {
            return pull % 2 == 0 ? DuelSeat.Target : DuelSeat.Gambler;
        }

        // Each seat takes its first pull before it may chicken out.
        public bool CanChickenOut(DuelSeat seat)
        {
            return Pulls > (seat == DuelSeat.Target ? 0 : 1);
        }

        // True when it fired.
        public bool Pull()
        {
            var seat = NextSeat;
            var fired = Pulls == BulletChamber;
            Pulls++;
            if (fired)
            {
                End(DuelEnding.Shot, seat);
            }

            return fired;
        }

        public void ChickenOut()
        {
            End(DuelEnding.ChickenedOut, NextSeat);
        }

        private void End(DuelEnding ending, DuelSeat seat)
        {
            Ending = ending;
            EndedBy = seat;
        }

        public override string ToString()
        {
            var ending = IsOver ? $"{EndedBy} {Ending}" : "still pulling";
            return $"Duel Roulette: {EmptyClicks} click(s), {ending}";
        }
    }
}
