namespace GARA.SkillCards.Rhythm
{
    // Which RhythmCompletionReport metric a RhythmSkillCard's base score
    // comes from, before the stray-press penalty is applied.
    public enum RhythmScoreSource
    {
        Accuracy,
        Completion,
        Blend
    }
}
