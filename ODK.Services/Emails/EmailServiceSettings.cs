namespace ODK.Services.Emails;

public class EmailServiceSettings
{
    public required string DefaultBodyBackground { get; init; }

    public required string DefaultBodyColor { get; init; }

    public required string DefaultHeaderBackground { get; init; }

    public required string DefaultHeaderColor { get; init; }
}