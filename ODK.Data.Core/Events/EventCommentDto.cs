using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Data.Core.Events;

public class EventCommentDto
{
    public required EventComment Comment { get; init; }

    public required Member Member { get; init; }
}