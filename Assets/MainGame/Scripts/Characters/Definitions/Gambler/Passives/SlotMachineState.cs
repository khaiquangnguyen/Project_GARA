using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    // The last spin, for displays to read.
    public sealed class SlotMachineState : PassiveRuntimeState
    {
        private SlotSymbol[] _lastSpin = System.Array.Empty<SlotSymbol>();

        public IReadOnlyList<SlotSymbol> LastSpin => _lastSpin;

        public bool LastSpinWasJackpot { get; private set; }

        public int JackpotCount { get; private set; }

        public void Record(SlotSymbol[] spin, bool jackpot)
        {
            _lastSpin = spin;
            LastSpinWasJackpot = jackpot;
            if (jackpot)
            {
                JackpotCount++;
            }
        }

        public override void Reset()
        {
            _lastSpin = System.Array.Empty<SlotSymbol>();
            LastSpinWasJackpot = false;
            JackpotCount = 0;
        }
    }
}
