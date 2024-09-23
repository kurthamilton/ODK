using ODK.Core.Issues;

namespace ODK.Services.Issues.Models;

public class IssuesPageViewModel
{
    public required IReadOnlyCollection<Issue> Issues { get; init; }
}
