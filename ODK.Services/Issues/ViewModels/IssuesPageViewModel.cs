using ODK.Core.Issues;

namespace ODK.Services.Issues.ViewModels;

public class IssuesPageViewModel
{
    public required IReadOnlyCollection<Issue> Issues { get; init; }
}
