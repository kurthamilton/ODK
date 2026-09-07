namespace ODK.Infrastructure.Settings;

/// <summary>
/// Who hosts the deployment, for the providers table in the privacy policy.
/// </summary>
/// <remarks>
/// The three travel together because the policy states all three of a provider - who it is, where it holds
/// the data, and where its own policy can be read - and a location that does not move with the name is how
/// a policy comes to state the wrong country after a migration.
/// </remarks>
public class PrivacyHostingSettings
{
    /// <summary>
    /// The country or area the data is held in, as the policy should read it: "the United Kingdom",
    /// "Germany". Empty where <see cref="Name"/> is empty, since the row is then not rendered.
    /// </summary>
    public required string Location { get; init; }

    public required string Name { get; init; }

    public required string PrivacyPolicyUrl { get; init; }
}
