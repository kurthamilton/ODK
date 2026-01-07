namespace ODK.Web.Common.Config.Settings;

public class AuthPasswordsSettings
{
    public required string Algorithm { get; init; }

    public required int Iterations { get; init; }
}
