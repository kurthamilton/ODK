using ODK.Core.Topics;

namespace ODK.Web.Razor.Models.Topics;

public class TopicPickerViewModel : TopicPickerFormSubmitViewModel
{    
    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
