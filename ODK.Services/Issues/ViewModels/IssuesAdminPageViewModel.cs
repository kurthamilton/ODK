using ODK.Core.Issues;

namespace ODK.Services.Issues.ViewModels;

public class IssuesAdminPageViewModel
{
    public required IReadOnlyCollection<Issue> Issues { get; init; }
}
