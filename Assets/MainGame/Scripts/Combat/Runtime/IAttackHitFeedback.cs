namespace GARA.Combat
{
    // Root of a hit-feedback prefab (see AttackExecutor.PlayHitFeedback).
    // Lets combat play it without referencing MoreMountains.Feedbacks.
    public interface IAttackHitFeedback
    {
        void Play();
    }
}
