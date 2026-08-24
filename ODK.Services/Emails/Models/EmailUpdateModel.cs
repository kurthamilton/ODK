namespace ODK.Services.Emails.Models;

public class EmailUpdateModel
{
    public required string HtmlContent { get; init; }

    public required bool IsGroupEmail { get; init; }

    public required string Subject { get; init; }
}
