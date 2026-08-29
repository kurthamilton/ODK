namespace ODK.Core.Platforms;

/// <summary>
/// The deployment a record belongs to. Paired with <see cref="PlatformType"/>: the two together name one
/// running instance of the app, which is the granularity a payment provider account is registered at.
/// </summary>
public enum EnvironmentType
{
    None = 0,
    Prod = 1,
    Dev = 2,
    E2E = 3
}
