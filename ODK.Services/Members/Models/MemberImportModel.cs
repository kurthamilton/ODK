namespace ODK.Services.Members.Models;

public class MemberImportModel
{
    public required string EmailAddress { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }
}