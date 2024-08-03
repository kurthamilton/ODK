namespace ODK.Services.Members;

public record MemberServiceSettings
{
    public required string ActivateAccountUrl { get; init; }

    public required string ConfirmEmailAddressUpdateUrl { get; init; }    
}
