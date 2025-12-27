using ODK.Core.Members;

namespace ODK.Core.Messages;

public class SiteContactMessageReply : IDatabaseEntity
{
    public Guid SiteContactMessageId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Member Member { get; set; } = null!;

    public Guid MemberId { get; set; }

    public string Message { get; set; } = string.Empty;
}
