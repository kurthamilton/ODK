using ODK.Core.Members;

namespace ODK.Core.Issues;

public class IssueMessage : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public Member Member { get; set; } = null!;

    public Guid MemberId { get; set; }

    public string Text { get; set; } = string.Empty;
}
