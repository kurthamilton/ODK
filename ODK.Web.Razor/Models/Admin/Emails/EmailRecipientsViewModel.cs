using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Emails;

public class EmailRecipientsViewModel
{
    public required EmailRecipientType RecipientType { get; init; }

    /// <summary>
    /// What the email resolves <c>{title}</c> to, itself a template.
    /// </summary>
    public required string Title { get; init; }
}
