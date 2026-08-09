using ODK.Core.Web;

namespace ODK.Services.Questions.ViewModels;

public class AboutPageViewModel
{
    public required IReadOnlyCollection<SiteQuestion> Questions { get; init; }
}
