namespace GARA.Input
{
    // A minigame player the Character Test window can make play itself.
    public interface IAutoPlayable
    {
        AutoPlayMode AutoPlay { get; set; }
    }
}
