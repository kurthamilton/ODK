namespace ODK.Infrastructure.Settings;

/// <summary>The email client an environment sends through.</summary>
/// <remarks>
/// Read by <c>DependencyRegistrar</c>, which resolves the one <c>IEmailClient</c> the app runs with, so this
/// reaches nothing outside this project. Implicit values: the name is what config states, and no number is
/// persisted, sent or queued anywhere.
/// </remarks>
public enum EmailClientType
{
    /// <summary>No client is named. Reserved, so an unset value never selects one.</summary>
    None,

    /// <summary>Brevo's transactional email API. What a deployed environment sends through.</summary>
    Brevo,

    /// <summary>Logs each email instead of sending it.</summary>
    Console,

    /// <summary>Delivers over SMTP to a local mail sink.</summary>
    Smtp
}
