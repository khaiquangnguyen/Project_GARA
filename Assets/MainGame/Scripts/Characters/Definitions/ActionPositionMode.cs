namespace GARA.Characters
{
    // Whether an action's animation plays right where the actor is already
    // standing, or the actor first dashes up to the target and back. None
    // means the concept doesn't apply at all — for a state whose whole job
    // IS the moving (MoveForwardState/MoveBackwardState), not a stance on
    // whether it moves. Appended at the end (not inserted before the
    // existing values) so already-serialized StayAtOriginalPosition (0) /
    // MoveInFrontOfEnemy (1) values on prefabs don't shift meaning.
    public enum ActionPositionMode
    {
        StayAtOriginalPosition,
        MoveInFrontOfEnemy,
        None
    }
}
