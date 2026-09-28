namespace GARA.ShakeBalance
{
    /// <summary>Why a <see cref="ShakeBalanceRunner"/> run ended.</summary>
    public enum ShakeBalanceEndReason
    {
        /// <summary>The meter filled in time.</summary>
        Succeeded,

        /// <summary>The value reached an edge.</summary>
        Fell,

        /// <summary>The definition's duration ran out before the meter filled.</summary>
        TimeUp,

        /// <summary>Cut short from outside.</summary>
        Aborted
    }
}
