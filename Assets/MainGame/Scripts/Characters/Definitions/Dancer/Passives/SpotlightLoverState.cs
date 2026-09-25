namespace GARA.Characters.Dancer
{
    public sealed class SpotlightLoverState : PassiveRuntimeState
    {
        public int encoreNotes;
        public int encoresTriggered;

        public override void Reset()
        {
            encoreNotes = 0;
            encoresTriggered = 0;
        }
    }
}
