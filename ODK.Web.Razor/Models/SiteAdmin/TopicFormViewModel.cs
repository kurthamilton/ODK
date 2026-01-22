using ODK.Core.Topics;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class TopicFormViewModel : TopicFormSubmitViewModel
{
    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }
}