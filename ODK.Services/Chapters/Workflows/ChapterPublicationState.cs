namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// How far a group has got towards being findable. Derived from the three dates that record it - its owner
/// submitting it, a site admin approving it, and its owner publishing it - and never stored.
/// </summary>
/// <remarks>
/// Numbered because a state or a trigger can travel as a background job argument, which Hangfire serialises
/// as the number: renumbering would have a job queued by one version run as something else under the next.
/// </remarks>
public enum ChapterPublicationState
{
    None = 0,

    /// <summary>Created, and still its owner's to finish. Nobody outside it can see it.</summary>
    Draft = 1,

    /// <summary>
    /// The owner has asked for it to be looked at, and it is waiting on a site admin. Numbered after the
    /// states that came before it rather than in the order it falls between them - see the remarks.
    /// </summary>
    Submitted = 4,

    /// <summary>A site admin has approved it. Its owner has still to publish it.</summary>
    Approved = 2,

    /// <summary>Published, so it can be found and joined.</summary>
    Published = 3
}
