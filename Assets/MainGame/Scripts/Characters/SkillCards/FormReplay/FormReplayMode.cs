namespace GARA.Characters
{
    // How a character replays another's skill in that character's form. The
    // skill plays the same either way; only who is seen doing it differs.
    public enum FormReplayMode
    {
        // The user falls back while a stand-in in the form performs it.
        StandIn,

        // The user takes the form itself for the skill.
        AssumeForm
    }
}
