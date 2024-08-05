namespace ODK.Services.Members;

public record MemberServiceSettings
{
    public required string ActivateAccountUrlPath { get; init; }

    public required string ConfirmEmailAddressUpdateUrlPath { get; init; }    
}
