namespace GARA.Characters
{
    // A live session whose steps play one after another: each waits for the
    // previous step's action to finish instead of cutting it off.
    public interface ISequentialLiveSkillInputSession : ILiveSkillInputSession
    {
    }
}
