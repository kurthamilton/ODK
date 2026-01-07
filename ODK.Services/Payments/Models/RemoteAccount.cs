namespace ODK.Services.Payments.Models;

public class RemoteAccount
{
    public required bool CardPaymentsEnabled { get; init; }

    public required string Id { get; init; }

    public required bool IdentityDocumentsProvided { get; init; }

    public required bool InitialOnboardingComplete { get; init; }
}
