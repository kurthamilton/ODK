using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Chapters;

public class CreateChapterFormViewModel : CreateChapterSubmitViewModel
{
    public required IReadOnlyCollection<Country> Countries { get; set; }
}
