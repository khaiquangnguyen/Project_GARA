namespace GARA.Characters.Bard
{
    public sealed class MoodSwingState : PassiveRuntimeState
    {
        // Null until the first swing; the passive's starting emotion until then.
        public Emotion? current;

        public override void Reset()
        {
            current = null;
        }
    }
}
