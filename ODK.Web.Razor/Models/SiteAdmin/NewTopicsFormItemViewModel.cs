namespace ODK.Web.Razor.Models.SiteAdmin;

public class NewTopicsFormItemViewModel
{
    public bool Approved { get; init; }

    public Guid NewTopicId { get; init; }

    public bool Rejected { get; init; }

    public string Topic { get; init; } = string.Empty;

    public string TopicGroup { get; init; } = string.Empty;
}