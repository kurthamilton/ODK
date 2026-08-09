using ODK.Core.Web;

namespace ODK.Services.Questions.ViewModels;

public class SiteQuestionsAdminPageViewModel
{
    public required IReadOnlyCollection<SiteQuestion> Questions { get; init; }
}
