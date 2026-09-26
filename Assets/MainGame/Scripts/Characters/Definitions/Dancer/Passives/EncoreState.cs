namespace GARA.Characters.Dancer
{
    public sealed class EncoreState : PassiveRuntimeState
    {
        public int stacks;
        public bool extraTurnPending;

        public override void Reset()
        {
            stacks = 0;
            extraTurnPending = false;
        }
    }
}
