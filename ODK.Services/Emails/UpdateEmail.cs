namespace ODK.Services.Emails;

public class UpdateEmail
{
    public required string HtmlContent { get; init; }

    public required bool Overridable { get; init; }

    public required string Subject { get; init; }
}
