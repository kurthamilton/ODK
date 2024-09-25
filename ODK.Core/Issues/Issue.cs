namespace ODK.Core.Issues;

public class Issue : IDatabaseEntity
{
    public DateTime? ClosedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public IssueStatusType Status { get; set; }

    public string Title { get; set; } = "";

    public IssueType Type { get; set; }
}
