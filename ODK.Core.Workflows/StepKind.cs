namespace ODK.Core.Workflows;

/// <summary>
/// What a step does to the world. The builder orders steps by it and the exporter renders an
/// irreversible step differently from a reversible one.
/// </summary>
public enum StepKind
{
    None = 0,

    /// <summary>Reads, validates or derives. Changes nothing.</summary>
    Decision,

    /// <summary>Stages changes in the unit of work.</summary>
    Write,

    /// <summary>Persists everything staged so far.</summary>
    Commit,

    /// <summary>
    /// An irreversible effect outside the database: an email, a queued background job, a call to a
    /// payment provider. One of these may not run while a write is still uncommitted.
    /// </summary>
    ExternalEffect
}
