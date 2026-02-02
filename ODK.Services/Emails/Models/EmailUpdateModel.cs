namespace ODK.Services.Emails.Models;

public class EmailUpdateModel
{
    public required string HtmlContent { get; init; }

    public required bool Overridable { get; init; }

    public required string Subject { get; init; }
}
