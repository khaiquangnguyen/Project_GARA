using GARA.Characters;

namespace GARA.Characters.Gambler
{
    // Runtime state for GamblingAddictPassive: chips accumulated from
    // skill-card resolutions and the roller/rng seams the passive rolls
    // through, so range-based effects and slot machine rolls are both
    // driven by the same deterministic-under-tests IGambleRandom.
    public sealed class GamblingAddictState : PassiveRuntimeState
    {
        public int chips;
        public int totalRolls;
        private IValueRangeRoller _roller;
        private IGambleRandom _rng;

        public IValueRangeRoller Roller => _roller ??= new GambleRangeRoller(Rng);
        public IGambleRandom Rng => _rng ??= new UnityGambleRandom();

        public override void Reset()
        {
            chips = 0;
            totalRolls = 0;
        }
    }
}
