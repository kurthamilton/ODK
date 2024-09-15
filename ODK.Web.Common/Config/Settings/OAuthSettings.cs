namespace ODK.Web.Common.Config.Settings;

public record OAuthSettings
{
    public required OAuthGoogleSettings Google { get; init; }
}
