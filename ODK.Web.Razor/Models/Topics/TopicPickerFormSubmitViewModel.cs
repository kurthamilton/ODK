namespace ODK.Web.Razor.Models.Topics;

public class TopicPickerFormSubmitViewModel
{
    public List<Guid>? TopicIds { get; init; }

    public List<string?>? NewTopicGroups { get; set; }

    public List<string?>? NewTopics { get; set; }
}
