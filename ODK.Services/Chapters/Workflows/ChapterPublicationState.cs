namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// How far a group has got towards being findable. Derived from the two dates that record it - a site admin
/// approving it, and its owner publishing it - and never stored.
/// </summary>
/// <remarks>
/// Numbered because a state or a trigger can travel as a background job argument, which Hangfire serialises
/// as the number: renumbering would have a job queued by one version run as something else under the next.
/// </remarks>
public enum ChapterPublicationState
{
    None = 0,

    /// <summary>Created, and waiting on a site admin. Nobody outside it can see it.</summary>
    Draft = 1,

    /// <summary>A site admin has approved it. Its owner has still to publish it.</summary>
    Approved = 2,

    /// <summary>Published, so it can be found and joined.</summary>
    Published = 3
}
