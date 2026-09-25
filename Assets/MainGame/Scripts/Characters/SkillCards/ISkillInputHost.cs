namespace GARA.Characters
{
    // The host a skill card's input session runs against — gives it access
    // to whatever driver component the concrete minigame needs (a rhythm
    // conductor, a shake detector, etc.) plus a MonoBehaviour to run
    // coroutines on, without the session needing to know what implements
    // this (e.g. AttackExecutor on the Combat side).
    public interface ISkillInputHost
    {
        T GetDriver<T>() where T : UnityEngine.Component;

        UnityEngine.MonoBehaviour CoroutineRunner { get; }

        // The character using the card.
        CharacterDefinition Actor { get; }
    }
}
