namespace ODK.E2E.Tests.Pages;

/// <summary>
/// One row of a member-import CSV, as a test states it.
/// </summary>
internal class MemberImportRow
{
    public required string EmailAddress { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }
}
