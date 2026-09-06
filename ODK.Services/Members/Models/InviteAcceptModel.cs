namespace ODK.Services.Members.Models;

public class InviteAcceptModel
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    /// <summary>The first password on the account, which is what makes it able to sign in.</summary>
    public required string Password { get; init; }

    /// <summary>The member's answers to the group's questions.</summary>
    public required IReadOnlyCollection<MemberPropertyUpdateModel> Properties { get; init; }

    public required string Token { get; init; }
}
