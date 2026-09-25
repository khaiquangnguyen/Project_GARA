namespace GARA.ShakeBalance
{
    /// <summary>Why a <see cref="ShakeBalanceRunner"/> run ended.</summary>
    public enum ShakeBalanceEndReason
    {
        /// <summary>The player pressed the cash out token, or the host stopped the run. Keeps the full result.</summary>
        CashedOut,

        /// <summary>The value reached an edge. Keeps only the definition's KeepOnFall fraction.</summary>
        Fell,

        /// <summary>The definition's duration ran out. Keeps the full result.</summary>
        TimeUp,

        /// <summary>Cut short from outside; the result still reflects what was banked.</summary>
        Aborted
    }
}
