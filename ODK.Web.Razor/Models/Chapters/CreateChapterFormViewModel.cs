using ODK.Core.Topics;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterFormViewModel : CreateChapterSubmitViewModel
{
    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
