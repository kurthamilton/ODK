namespace ODK.Services.Members.Models;

public class MemberImportModel
{
    public required string EmailAddress { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public static IReadOnlyCollection<string> GetCsvHeaderRow() =>
    [
        nameof(FirstName),
        nameof(LastName),
        nameof(EmailAddress)
    ];
}