namespace ODK.Web.Razor.Models.SuperAdmin;

public class NewTopicsFormItemViewModel
{
    public bool Approved { get; init; }

    public Guid NewTopicId { get; init; }

    public bool Rejected { get; init; }

    public string Topic { get; init; } = "";

    public string TopicGroup { get; init; } = "";
}
