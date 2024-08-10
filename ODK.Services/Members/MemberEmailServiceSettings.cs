namespace ODK.Services.Members;

public record MemberEmailServiceSettings
{
    public required string ActivateAccountUrlPath { get; init; }

    public required string ConfirmEmailAddressUpdateUrlPath { get; init; }
}
