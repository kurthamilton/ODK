using ODK.Core.Issues;

namespace ODK.Services.Issues.Models;

public class IssueCreateModel
{
    public required string Message { get; init; }

    public required IssueType Type { get; init; }

    public required string Title { get; init; }
}
