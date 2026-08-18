using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters.Workflows;

/// <summary>
/// Everything the publication machine's state resolver and its steps read. Small, because the state is two
/// dates on the group itself - the only thing a transition needs beyond that is who to tell.
/// </summary>
public sealed class ChapterPublicationContext
{
    public required Chapter Chapter { get; init; }

    /// <summary>The group's owner, who is told when it is approved. Not needed to publish.</summary>
    public Member? Owner { get; init; }

    public required IServiceRequest Request { get; init; }

    /// <summary>The owner, on a transition that has to tell them something.</summary>
    public Member RequiredOwner => Owner ?? throw new InvalidOperationException(
        "The transition notifies the group's owner but none was resolved");
}
