namespace GARA.InputSets
{
    /// <summary>
    /// Why a set attempt failed. ChordSpread and ExtraInput exist here for reuse by the
    /// Chords system; InputSets itself never produces them.
    /// </summary>
    public enum InputSetFailureReason
    {
        None,
        WrongInput,
        SetTimedOut,
        SetCollectionTimedOut,
        Aborted,
        AttemptsExhausted,
        ChordSpread,
        ExtraInput
    }
}
