namespace ODK.Infrastructure.Settings;

public record OAuthSettings
{
    public required OAuthGoogleSettings Google { get; init; }
}
