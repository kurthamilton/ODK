namespace ODK.Core.Issues;

public class IssueMessage : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public Guid MemberId { get; set; }

    public string Text { get; set; } = "";
}
