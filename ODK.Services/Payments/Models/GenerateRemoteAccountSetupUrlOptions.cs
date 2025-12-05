namespace ODK.Services.Payments.Models;

public class GenerateRemoteAccountSetupUrlOptions
{
    public required string Id { get; init; }

    public required string RefreshUrl { get; init; }

    public required string ReturnUrl { get; init; }
}
