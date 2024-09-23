using ODK.Core.Issues;

namespace ODK.Services.Issues.Models;

public class IssuesAdminPageViewModel
{
    public required IReadOnlyCollection<Issue> Issues { get; init; }
}
