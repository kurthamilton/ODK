using ODK.Core.Countries;
using ODK.Core.Topics;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterFormViewModel : CreateChapterSubmitViewModel
{
    public required IReadOnlyCollection<Country> Countries { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
